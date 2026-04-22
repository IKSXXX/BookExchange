using System.ComponentModel.DataAnnotations;

namespace BookExchange.Db.Entities;

/// <summary>
/// Заявка на обмен: Sender хочет <see cref="BookRequested"/>, а взамен предлагает <see cref="BookOffered"/>
/// (или ничего — "безвозмездно"). Receiver — владелец запрашиваемой книги.
/// </summary>
public class ExchangeRequest : BaseEntity
{
    /// <summary>Книга, которую предлагает отправитель. <c>null</c> = отдаёт безвозмездно / просит подарок.</summary>
    public int? BookOfferedId { get; set; }
    public Book? BookOffered { get; set; }

    /// <summary>Книга, которую отправитель хочет получить.</summary>
    [Required]
    public int BookRequestedId { get; set; }
    public Book? BookRequested { get; set; }

    [Required]
    public string SenderId { get; set; } = string.Empty;
    public User? Sender { get; set; }

    [Required]
    public string ReceiverId { get; set; } = string.Empty;
    public User? Receiver { get; set; }

    public ExchangeStatus Status { get; set; } = ExchangeStatus.Pending;

    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
