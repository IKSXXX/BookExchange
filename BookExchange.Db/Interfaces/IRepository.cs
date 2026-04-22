using System.Linq.Expressions;

namespace BookExchange.Db.Interfaces;

/// <summary>
/// Обобщённый репозиторий для любой сущности. Позволяет не дублировать типовые запросы
/// (GetById, GetAll, Find и т.д.) в каждом сервисе.
/// </summary>
/// <typeparam name="T">Тип сущности (класс с набором свойств).</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>Найти сущность по ключу. Возвращает null, если нет.</summary>
    Task<T?> GetByIdAsync(object id);

    /// <summary>Получить все сущности (осторожно на больших таблицах!).</summary>
    Task<IReadOnlyList<T>> GetAllAsync();

    /// <summary>
    /// Вернуть сырой <see cref="IQueryable{T}"/> для построения сложных запросов (фильтры, пагинация).
    /// Контроллер/сервис сам сделает ToListAsync.
    /// </summary>
    IQueryable<T> Query();

    /// <summary>Найти по предикату (lambda).</summary>
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>Проверить существование.</summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
}
