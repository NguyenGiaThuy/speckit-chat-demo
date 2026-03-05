using System.Text.Json;
using MessagingService.Data;
using MessagingService.Data.Entities;
using MessagingService.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MessagingService.Services;

public class MessageService
{
    private readonly AppDbContext _db;
    private readonly IEventPublisher _pubSub;

    public MessageService(AppDbContext db, IEventPublisher pubSub)
    {
        _db = db;
        _pubSub = pubSub;
    }

    public async Task<(bool success, string? error, MessageDto? result)> SendAsync(
        Guid channelId, Guid authorId, SendMessageRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Content))
            return (false, "content_required", null);

        if (req.Content.Length > 2000)
            return (false, "content_too_long", null);

        // Validate reply_to_id if provided
        if (req.ReplyToId.HasValue)
        {
            var parent = await _db.Messages.FindAsync(req.ReplyToId.Value);
            if (parent == null || parent.Deleted)
                return (false, "reply_target_not_found", null);
        }

        var message = new Message
        {
            ChannelId = channelId,
            AuthorId = authorId,
            Content = req.Content.Trim(),
            ReplyToId = req.ReplyToId
        };

        _db.Messages.Add(message);
        await _db.SaveChangesAsync();

        var dto = await LoadDtoAsync(message.Id);
        await _pubSub.PublishAsync(channelId, new WsEvent("message.created", dto!));
        return (true, null, dto);
    }

    public async Task<MessagePageDto> GetPageAsync(Guid channelId, Guid? cursor, int limit = 50)
    {
        limit = Math.Clamp(limit, 1, 100);

        var query = _db.Messages
            .Where(m => m.ChannelId == channelId)
            .OrderByDescending(m => m.CreatedAt)
            .AsQueryable();

        if (cursor.HasValue)
        {
            var cursorMsg = await _db.Messages.FindAsync(cursor.Value);
            if (cursorMsg != null)
                query = query.Where(m => m.CreatedAt < cursorMsg.CreatedAt);
        }

        var messages = await query
            .Take(limit + 1)
            .Include(m => m.ReplyTo)
            .ToListAsync();

        Guid? nextCursor = null;
        if (messages.Count > limit)
        {
            messages = messages.Take(limit).ToList();
            nextCursor = messages.Last().Id;
        }

        return new MessagePageDto
        {
            Messages = messages.Select(MapToDto).ToList(),
            NextCursor = nextCursor
        };
    }

    public async Task<(bool success, string? error, MessageDto? result)> EditAsync(
        Guid messageId, Guid requesterId, EditMessageRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Content))
            return (false, "content_required", null);

        if (req.Content.Length > 2000)
            return (false, "content_too_long", null);

        var message = await _db.Messages.FindAsync(messageId);
        if (message == null || message.Deleted) return (false, "not_found", null);
        if (message.AuthorId != requesterId) return (false, "forbidden", null);

        message.Content = req.Content.Trim();
        message.EditedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var dto = MapToDto(message);
        await _pubSub.PublishAsync(message.ChannelId, new WsEvent("message.updated", dto));
        return (true, null, dto);
    }

    public async Task<(bool success, string? error)> DeleteAsync(Guid messageId, Guid requesterId)
    {
        var message = await _db.Messages.FindAsync(messageId);
        if (message == null || message.Deleted) return (false, "not_found");
        if (message.AuthorId != requesterId) return (false, "forbidden");

        message.Deleted = true;
        await _db.SaveChangesAsync();

        await _pubSub.PublishAsync(message.ChannelId, new WsEvent("message.deleted", new { id = messageId }));
        return (true, null);
    }

    private async Task<MessageDto?> LoadDtoAsync(Guid id)
    {
        var msg = await _db.Messages.Include(m => m.ReplyTo).FirstOrDefaultAsync(m => m.Id == id);
        return msg == null ? null : MapToDto(msg);
    }

    public static MessageDto MapToDto(Message m) => new()
    {
        Id = m.Id,
        ChannelId = m.ChannelId,
        AuthorId = m.AuthorId,
        Content = m.Deleted ? "[message deleted]" : m.Content,
        ReplyToId = m.ReplyToId,
        ReplyToSnippet = m.ReplyTo != null ? MapToDto(m.ReplyTo) : null,
        Deleted = m.Deleted,
        CreatedAt = m.CreatedAt,
        EditedAt = m.EditedAt
    };
}

public record WsEvent(string Type, object Data);
