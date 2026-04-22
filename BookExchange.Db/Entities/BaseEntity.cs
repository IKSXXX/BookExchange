using System.ComponentModel.DataAnnotations;

namespace BookExchange.Db.Entities;

/// <summary>
/// Базовый класс для всех сущностей с числовым Id.
/// Содержит общие поля <see cref="CreatedAt"/> и <see cref="UpdatedAt"/> — это удобно
/// для сортировки "новые сверху" и для аудита.
/// </summary>
/// <remarks>
/// ВАЖНО: <see cref="User"/> наследуется от <c>IdentityUser</c> (его Id — string),
/// поэтому User НЕ наследует этот BaseEntity. Чтобы User всё-таки имел поле
/// <c>CreatedAt</c>, оно добавлено у него напрямую.
/// </remarks>
public abstract class BaseEntity
{
    /// <summary>Суррогатный первичный ключ. EF Core автоматически сделает identity-колонку.</summary>
    [Key]
    public int Id { get; set; }

    /// <summary>Дата создания записи (UTC). Заполняется при Add в <c>SaveChangesAsync</c>.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Дата последнего изменения (UTC). Заполняется при Update в <c>SaveChangesAsync</c>.</summary>
    public DateTime? UpdatedAt { get; set; }
}
