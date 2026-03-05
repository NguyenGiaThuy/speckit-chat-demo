namespace MessagingService.DTOs;

public class SendMessageRequest
{
    public string Content { get; set; } = string.Empty;
    public Guid? ReplyToId { get; set; }
}

public class EditMessageRequest
{
    public string Content { get; set; } = string.Empty;
}

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public Guid AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid? ReplyToId { get; set; }
    public MessageDto? ReplyToSnippet { get; set; }
    public bool Deleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
}

public class MessagePageDto
{
    public List<MessageDto> Messages { get; set; } = new();
    public Guid? NextCursor { get; set; }
}
