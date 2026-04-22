namespace BookExchange.Db.Entities;

/// <summary>Книга, которую пользователь хочет получить (wishlist).</summary>
public class WishlistItem : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }

    public int BookId { get; set; }
    public Book? Book { get; set; }
}
