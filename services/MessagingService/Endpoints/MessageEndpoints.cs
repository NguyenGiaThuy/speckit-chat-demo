using System.Security.Claims;
using MessagingService.DTOs;
using MessagingService.Services;

namespace MessagingService.Endpoints;

public static class MessageEndpoints
{
    public static void MapMessageEndpoints(this WebApplication app)
    {
        // POST /channels/{channelId}/messages
        app.MapPost("/channels/{channelId:guid}/messages",
            async (Guid channelId, SendMessageRequest req, ClaimsPrincipal user, MessageService svc) =>
            {
                var authorId = GetUserId(user);
                if (authorId == null) return Results.Unauthorized();

                var (success, error, result) = await svc.SendAsync(channelId, authorId.Value, req);
                if (!success)
                    return error is "content_required" or "content_too_long"
                        ? Results.UnprocessableEntity(new { error })
                        : Results.BadRequest(new { error });

                return Results.Created($"/channels/{channelId}/messages/{result!.Id}", result);
            }).RequireAuthorization();

        // GET /channels/{channelId}/messages
        app.MapGet("/channels/{channelId:guid}/messages",
            async (Guid channelId, Guid? cursor, int limit, ClaimsPrincipal user, MessageService svc) =>
            {
                if (GetUserId(user) == null) return Results.Unauthorized();
                var page = await svc.GetPageAsync(channelId, cursor, limit == 0 ? 50 : limit);
                return Results.Ok(page);
            }).RequireAuthorization();

        // PUT /messages/{id}
        app.MapPut("/messages/{id:guid}",
            async (Guid id, EditMessageRequest req, ClaimsPrincipal user, MessageService svc) =>
            {
                var requesterId = GetUserId(user);
                if (requesterId == null) return Results.Unauthorized();

                var (success, error, result) = await svc.EditAsync(id, requesterId.Value, req);
                return (success, error) switch
                {
                    (true, _)            => Results.Ok(result),
                    (_, "forbidden")     => Results.Forbid(),
                    (_, "not_found")     => Results.NotFound(),
                    _                    => Results.UnprocessableEntity(new { error })
                };
            }).RequireAuthorization();

        // DELETE /messages/{id}
        app.MapDelete("/messages/{id:guid}",
            async (Guid id, ClaimsPrincipal user, MessageService svc) =>
            {
                var requesterId = GetUserId(user);
                if (requesterId == null) return Results.Unauthorized();

                var (success, error) = await svc.DeleteAsync(id, requesterId.Value);
                return (success, error) switch
                {
                    (true, _)            => Results.NoContent(),
                    (_, "forbidden")     => Results.Forbid(),
                    _                    => Results.NotFound()
                };
            }).RequireAuthorization();
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
