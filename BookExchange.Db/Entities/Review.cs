using System.ComponentModel.DataAnnotations;

namespace BookExchange.Db.Entities;

/// <summary>
/// Отзыв пользователя о другом пользователе после завершённого обмена.
/// Используется для расчёта User.Rating.
/// </summary>
public class Review : BaseEntity
{
    [Required]
    public string FromUserId { get; set; } = string.Empty;
    public User? FromUser { get; set; }

    [Required]
    public string ToUserId { get; set; } = string.Empty;
    public User? ToUser { get; set; }

    [Required]
    public int ExchangeRequestId { get; set; }
    public ExchangeRequest? ExchangeRequest { get; set; }

    /// <summary>Оценка от 1 до 5.</summary>
    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }
}
