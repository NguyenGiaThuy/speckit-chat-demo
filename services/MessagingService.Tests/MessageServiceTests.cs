using MessagingService.Data;
using MessagingService.Data.Entities;
using MessagingService.DTOs;
using MessagingService.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MessagingService.Tests;

public class MessageServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly Mock<IEventPublisher> _pubSubMock;
    private readonly MessageService _svc;

    public MessageServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);

        _pubSubMock = new Mock<IEventPublisher>();
        _pubSubMock.Setup(p => p.PublishAsync(It.IsAny<Guid>(), It.IsAny<WsEvent>()))
            .Returns(Task.CompletedTask);

        _svc = new MessageService(_db, _pubSubMock.Object);
    }

    public void Dispose() => _db.Dispose();

    // ── Send ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Send_ValidMessage_ReturnsDtoAndPublishesEvent()
    {
        var channelId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var (success, _, result) = await _svc.SendAsync(channelId, authorId,
            new SendMessageRequest { Content = "Hello world" });

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal("Hello world", result.Content);
        Assert.Equal(channelId, result.ChannelId);
        Assert.Equal(authorId, result.AuthorId);

        _pubSubMock.Verify(p => p.PublishAsync(channelId,
            It.Is<WsEvent>(e => e.Type == "message.created")), Times.Once);
    }

    [Fact]
    public async Task Send_EmptyContent_ReturnsError()
    {
        var (success, error, _) = await _svc.SendAsync(Guid.NewGuid(), Guid.NewGuid(),
            new SendMessageRequest { Content = "   " });

        Assert.False(success);
        Assert.Equal("content_required", error);
    }

    [Fact]
    public async Task Send_ContentTooLong_ReturnsError()
    {
        var (success, error, _) = await _svc.SendAsync(Guid.NewGuid(), Guid.NewGuid(),
            new SendMessageRequest { Content = new string('a', 2001) });

        Assert.False(success);
        Assert.Equal("content_too_long", error);
    }

    [Fact]
    public async Task Send_WithReplyTo_SetsParentReference()
    {
        var channelId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        // Create parent message directly
        var parent = new Message { ChannelId = channelId, AuthorId = authorId, Content = "Parent" };
        _db.Messages.Add(parent);
        await _db.SaveChangesAsync();

        var (success, _, result) = await _svc.SendAsync(channelId, authorId,
            new SendMessageRequest { Content = "Reply", ReplyToId = parent.Id });

        Assert.True(success);
        Assert.Equal(parent.Id, result!.ReplyToId);
        Assert.NotNull(result.ReplyToSnippet);
    }

    [Fact]
    public async Task Send_InvalidReplyToId_ReturnsError()
    {
        var (success, error, _) = await _svc.SendAsync(Guid.NewGuid(), Guid.NewGuid(),
            new SendMessageRequest { Content = "Reply", ReplyToId = Guid.NewGuid() });

        Assert.False(success);
        Assert.Equal("reply_target_not_found", error);
    }

    // ── Get Page ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPage_ReturnsMessagesNewestFirst()
    {
        var channelId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        for (int i = 0; i < 5; i++)
        {
            _db.Messages.Add(new Message
            {
                ChannelId = channelId, AuthorId = authorId,
                Content = $"msg {i}",
                CreatedAt = DateTime.UtcNow.AddMinutes(i)
            });
        }
        await _db.SaveChangesAsync();

        var page = await _svc.GetPageAsync(channelId, null, 50);

        Assert.Equal(5, page.Messages.Count);
        Assert.True(page.Messages[0].CreatedAt >= page.Messages[1].CreatedAt);
    }

    [Fact]
    public async Task GetPage_CursorPagination_ReturnsNextPage()
    {
        var channelId = Guid.NewGuid();
        for (int i = 0; i < 7; i++)
        {
            _db.Messages.Add(new Message
            {
                ChannelId = channelId, AuthorId = Guid.NewGuid(),
                Content = $"msg {i}",
                CreatedAt = DateTime.UtcNow.AddMinutes(i)
            });
        }
        await _db.SaveChangesAsync();

        var page1 = await _svc.GetPageAsync(channelId, null, 3);
        Assert.Equal(3, page1.Messages.Count);
        Assert.NotNull(page1.NextCursor);

        var page2 = await _svc.GetPageAsync(channelId, page1.NextCursor, 3);
        Assert.True(page2.Messages.Count > 0);

        // No overlap between pages
        var ids1 = page1.Messages.Select(m => m.Id).ToHashSet();
        Assert.All(page2.Messages, m => Assert.DoesNotContain(m.Id, ids1));
    }

    [Fact]
    public async Task GetPage_DeletedMessages_ShowPlaceholder()
    {
        var channelId = Guid.NewGuid();
        _db.Messages.Add(new Message
        {
            ChannelId = channelId, AuthorId = Guid.NewGuid(),
            Content = "Secret", Deleted = true
        });
        await _db.SaveChangesAsync();

        var page = await _svc.GetPageAsync(channelId, null, 50);

        Assert.Equal("[message deleted]", page.Messages[0].Content);
    }

    // ── Edit ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Edit_ValidRequest_UpdatesContent()
    {
        var message = await SeedMessage();

        var (success, _, result) = await _svc.EditAsync(message.Id, message.AuthorId,
            new EditMessageRequest { Content = "Updated content" });

        Assert.True(success);
        Assert.Equal("Updated content", result!.Content);
        Assert.NotNull(result.EditedAt);
        _pubSubMock.Verify(p => p.PublishAsync(message.ChannelId,
            It.Is<WsEvent>(e => e.Type == "message.updated")), Times.Once);
    }

    [Fact]
    public async Task Edit_WrongAuthor_ReturnsForbidden()
    {
        var message = await SeedMessage();

        var (success, error, _) = await _svc.EditAsync(message.Id, Guid.NewGuid(),
            new EditMessageRequest { Content = "Hack" });

        Assert.False(success);
        Assert.Equal("forbidden", error);
    }

    [Fact]
    public async Task Edit_NotFound_ReturnsError()
    {
        var (success, error, _) = await _svc.EditAsync(Guid.NewGuid(), Guid.NewGuid(),
            new EditMessageRequest { Content = "X" });

        Assert.False(success);
        Assert.Equal("not_found", error);
    }

    [Fact]
    public async Task Edit_EmptyContent_ReturnsError()
    {
        var msg = await SeedMessage();

        var (success, error, _) = await _svc.EditAsync(msg.Id, msg.AuthorId,
            new EditMessageRequest { Content = "" });

        Assert.False(success);
        Assert.Equal("content_required", error);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ValidRequest_SoftDeletes()
    {
        var message = await SeedMessage();

        var (success, _) = await _svc.DeleteAsync(message.Id, message.AuthorId);

        Assert.True(success);
        var dbMsg = await _db.Messages.FindAsync(message.Id);
        Assert.True(dbMsg!.Deleted);
        _pubSubMock.Verify(p => p.PublishAsync(message.ChannelId,
            It.Is<WsEvent>(e => e.Type == "message.deleted")), Times.Once);
    }

    [Fact]
    public async Task Delete_WrongAuthor_ReturnsForbidden()
    {
        var message = await SeedMessage();

        var (success, error) = await _svc.DeleteAsync(message.Id, Guid.NewGuid());

        Assert.False(success);
        Assert.Equal("forbidden", error);
    }

    [Fact]
    public async Task Delete_AlreadyDeleted_ReturnsNotFound()
    {
        var message = await SeedMessage(deleted: true);

        var (success, error) = await _svc.DeleteAsync(message.Id, message.AuthorId);

        Assert.False(success);
        Assert.Equal("not_found", error);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<Message> SeedMessage(string content = "Hello", bool deleted = false)
    {
        var msg = new Message
        {
            ChannelId = Guid.NewGuid(),
            AuthorId = Guid.NewGuid(),
            Content = content,
            Deleted = deleted
        };
        _db.Messages.Add(msg);
        await _db.SaveChangesAsync();
        return msg;
    }
}
