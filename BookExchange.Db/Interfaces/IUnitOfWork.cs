using BookExchange.Db.Entities;

namespace BookExchange.Db.Interfaces;

/// <summary>
/// Единица работы (Unit of Work) — объединяет несколько репозиториев под одной
/// транзакцией <c>SaveChangesAsync</c>. В контроллерах мы инъектируем именно <c>IUnitOfWork</c>,
/// чтобы не тащить десяток отдельных репозиториев.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    IRepository<Book> Books { get; }
    IRepository<ExchangeRequest> Exchanges { get; }
    IRepository<Message> Messages { get; }
    IRepository<Review> Reviews { get; }
    IRepository<WishlistItem> Wishlist { get; }
    IRepository<Favorite> Favorites { get; }
    IRepository<Discussion> Discussions { get; }
    IRepository<DiscussionMessage> DiscussionMessages { get; }
    IRepository<QuizQuestion> QuizQuestions { get; }
    IRepository<BookOfTheDay> BooksOfTheDay { get; }

    /// <summary>Зафиксировать все отложенные изменения в БД. Возвращает число затронутых строк.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
