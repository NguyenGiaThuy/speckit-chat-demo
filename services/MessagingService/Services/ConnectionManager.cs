using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace MessagingService.Services;

public class ConnectionManager
{
    // connectionId → (socket, set of subscribed channelIds)
    private readonly ConcurrentDictionary<Guid, (WebSocket Socket, HashSet<Guid> Channels)> _connections = new();

    public Guid Register(WebSocket socket)
    {
        var id = Guid.NewGuid();
        _connections[id] = (socket, new HashSet<Guid>());
        return id;
    }

    public void Unregister(Guid connectionId)
    {
        _connections.TryRemove(connectionId, out _);
    }

    public void Subscribe(Guid connectionId, Guid channelId)
    {
        if (_connections.TryGetValue(connectionId, out var entry))
            entry.Channels.Add(channelId);
    }

    public void BroadcastToChannel(Guid channelId, string message)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(bytes);

        foreach (var (_, (socket, channels)) in _connections)
        {
            if (!channels.Contains(channelId)) continue;
            if (socket.State != WebSocketState.Open) continue;

            // Fire-and-forget; errors are swallowed to avoid killing the broadcast
            _ = socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None)
                .ContinueWith(t => { }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
