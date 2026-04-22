namespace BookExchange.Db.Entities;

/// <summary>Избранная (лайкнутая) книга пользователя.</summary>
public class Favorite : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }

    public int BookId { get; set; }
    public Book? Book { get; set; }
}
