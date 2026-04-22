namespace BookExchange.Db.Entities;

/// <summary>
/// Состояние книги. Хранится в БД как int (по умолчанию у EF Core для enum).
/// </summary>
public enum BookCondition
{
    /// <summary>Как новая, без следов использования.</summary>
    Excellent = 1,

    /// <summary>Хорошее: лёгкий износ, нет повреждений.</summary>
    Good = 2,

    /// <summary>Приемлемое: есть следы чтения, но всё читается.</summary>
    Acceptable = 3
}

/// <summary>
/// Статус заявки на обмен. Меняется в процессе жизни заявки.
/// Диаграмма состояний:
/// Pending → Accepted → Completed
///         → Rejected
/// Pending → Cancelled (отменил отправитель)
/// </summary>
public enum ExchangeStatus
{
    /// <summary>Ожидает решения получателя.</summary>
    Pending = 0,

    /// <summary>Принята получателем.</summary>
    Accepted = 1,

    /// <summary>Отклонена получателем.</summary>
    Rejected = 2,

    /// <summary>Завершена (книги переданы друг другу).</summary>
    Completed = 3,

    /// <summary>Отменена отправителем до принятия.</summary>
    Cancelled = 4
}
