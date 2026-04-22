using System.ComponentModel.DataAnnotations;

namespace BookExchange.Db.Entities;

/// <summary>
/// Сообщение в чате заявки на обмен. Отправляется/принимается через SignalR ChatHub,
/// а в БД сохраняется для истории.
/// </summary>
public class Message : BaseEntity
{
    [Required]
    public int ExchangeRequestId { get; set; }
    public ExchangeRequest? ExchangeRequest { get; set; }

    [Required]
    public string SenderId { get; set; } = string.Empty;
    public User? Sender { get; set; }

    [Required, MaxLength(2000)]
    public string Text { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
