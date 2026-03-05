using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using MessagingService.Services;
using WsManager = MessagingService.Services.ConnectionManager;

namespace MessagingService.Endpoints;

public static class WebSocketEndpoint
{
    public static void MapWebSocketEndpoint(this WebApplication app)
    {
        app.Map("/ws", async (HttpContext ctx, MessagingService.Services.ConnectionManager wsManager) =>
        {
            if (!ctx.WebSockets.IsWebSocketRequest)
            {
                ctx.Response.StatusCode = 400;
                return;
            }

            // JWT auth via ?token= query param
            var token = ctx.Request.Query["token"].FirstOrDefault();
            if (string.IsNullOrEmpty(token))
            {
                ctx.Response.StatusCode = 401;
                return;
            }

            // Validate JWT and build ClaimsPrincipal
            ClaimsPrincipal? principal;
            try
            {
                var authService = ctx.RequestServices.GetRequiredService<ITokenValidator>();
                principal = authService.Validate(token);
            }
            catch
            {
                ctx.Response.StatusCode = 401;
                return;
            }

            if (principal == null)
            {
                ctx.Response.StatusCode = 401;
                return;
            }

            var socket = await ctx.WebSockets.AcceptWebSocketAsync();
            var connectionId = wsManager.Register(socket);

            try
            {
                await HandleAsync(socket, connectionId, wsManager);
            }
            finally
            {
                wsManager.Unregister(connectionId);
            }
        });
    }

    private static async Task HandleAsync(WebSocket socket, Guid connectionId, WsManager wsManager)
    {
        var buffer = new byte[4096];

        while (socket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result;
            using var ms = new MemoryStream();

            do
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                    return;
                }
                ms.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);

            var json = Encoding.UTF8.GetString(ms.ToArray());

            try
            {
                var msg = JsonSerializer.Deserialize<WsClientMessage>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (msg?.Action == "subscribe" && msg.ChannelId.HasValue)
                    wsManager.Subscribe(connectionId, msg.ChannelId.Value);
            }
            catch { /* ignore malformed messages */ }
        }
    }

    private record WsClientMessage(string Action, Guid? ChannelId);
}
