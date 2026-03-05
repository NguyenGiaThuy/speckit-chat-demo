using System.ComponentModel.DataAnnotations;

namespace MessagingService.Data.Entities;

public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ChannelId { get; set; }

    public Guid AuthorId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public Guid? ReplyToId { get; set; }
    public Message? ReplyTo { get; set; }

    public bool Deleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }
}
