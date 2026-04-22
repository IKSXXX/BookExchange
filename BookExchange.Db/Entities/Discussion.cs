using System.ComponentModel.DataAnnotations;

namespace BookExchange.Db.Entities;

/// <summary>Тема обсуждения к книге (общий чат для всех пользователей).</summary>
public class Discussion : BaseEntity
{
    [Required]
    public int BookId { get; set; }
    public Book? Book { get; set; }

    /// <summary>Автор темы.</summary>
    [Required]
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }

    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    public ICollection<DiscussionMessage> Messages { get; set; } = new List<DiscussionMessage>();
}
