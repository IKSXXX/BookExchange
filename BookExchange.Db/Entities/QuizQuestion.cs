using System.ComponentModel.DataAnnotations;

namespace BookExchange.Db.Entities;

/// <summary>
/// Вопрос мини-игры "Угадай книгу по цитате" на главной странице.
/// </summary>
public class QuizQuestion : BaseEntity
{
    /// <summary>Книга, из которой цитата (она же правильный ответ, её заголовок).</summary>
    [Required]
    public int BookId { get; set; }
    public Book? Book { get; set; }

    [Required, MaxLength(1000)]
    public string Quote { get; set; } = string.Empty;

    /// <summary>Правильный ответ (обычно название книги — копия из Book.Title на момент создания).</summary>
    [Required, MaxLength(300)]
    public string CorrectAnswer { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string Option2 { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string Option3 { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string Option4 { get; set; } = string.Empty;
}
