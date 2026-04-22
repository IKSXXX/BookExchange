namespace BookExchange.Db.Entities;

/// <summary>
/// Книга дня. На каждую дату — максимум одна запись (уникальный индекс по Date).
/// Можно выбирать вручную из админки или автоматически.
/// </summary>
public class BookOfTheDay : BaseEntity
{
    public int BookId { get; set; }
    public Book? Book { get; set; }

    /// <summary>Дата (без времени), на которую назначена эта книга.</summary>
    public DateTime Date { get; set; }
}
