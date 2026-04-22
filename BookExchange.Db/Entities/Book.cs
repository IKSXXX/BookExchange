using System.ComponentModel.DataAnnotations;

namespace BookExchange.Db.Entities;

/// <summary>
/// Книга, которую пользователь готов обменять.
/// </summary>
public class Book : BaseEntity
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Author { get; set; } = string.Empty;

    /// <summary>ISBN. Не уникальный, т.к. одна и та же книга может быть у разных людей.</summary>
    [MaxLength(32)]
    public string? ISBN { get; set; }

    [MaxLength(4000)]
    public string? Description { get; set; }

    /// <summary>Путь к обложке: относительный (/images/books/...) или полноценный URL.</summary>
    [MaxLength(500)]
    public string? CoverImagePath { get; set; }

    [MaxLength(100)]
    public string Genre { get; set; } = "Другое";

    public BookCondition Condition { get; set; } = BookCondition.Good;

    /// <summary>Год издания (необязательно).</summary>
    public int? Year { get; set; }

    /// <summary>Язык книги, например "Русский".</summary>
    [MaxLength(60)]
    public string? Language { get; set; } = "Русский";

    /// <summary>FK на владельца. string, т.к. Id у User — GUID-строка.</summary>
    [Required]
    public string OwnerId { get; set; } = string.Empty;
    public User? Owner { get; set; }

    /// <summary>Доступна ли книга для обмена (если идёт активная заявка — false).</summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>Скрыта ли книга модератором (не показывается в каталоге).</summary>
    public bool IsHidden { get; set; }

    public ICollection<Discussion> Discussions { get; set; } = new List<Discussion>();
    public ICollection<WishlistItem> WishlistedBy { get; set; } = new List<WishlistItem>();
    public ICollection<Favorite> FavoritedBy { get; set; } = new List<Favorite>();
    public ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
}
