using System.ComponentModel.DataAnnotations;

namespace BookExchange.Db.Entities;

/// <summary>Сообщение в теме обсуждения. Приходит через DiscussionHub SignalR.</summary>
public class DiscussionMessage : BaseEntity
{
    [Required]
    public int DiscussionId { get; set; }
    public Discussion? Discussion { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }

    [Required, MaxLength(2000)]
    public string Text { get; set; } = string.Empty;
}
