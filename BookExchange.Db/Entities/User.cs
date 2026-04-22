using Microsoft.AspNetCore.Identity;

namespace BookExchange.Db.Entities;

/// <summary>
/// Пользователь приложения. Наследуется от <see cref="IdentityUser"/>,
/// чтобы ASP.NET Core Identity взял на себя: хеширование паролей, токены,
/// роли, подтверждение email и т.д.
/// </summary>
/// <remarks>
/// ВАЖНО: Id у <see cref="IdentityUser"/> — строка (GUID). Поэтому все FK на User — <c>string</c>.
/// </remarks>
public class User : IdentityUser
{
    /// <summary>Относительный путь к аватару в <c>wwwroot/images/avatars/</c> или URL.</summary>
    public string? AvatarPath { get; set; }

    /// <summary>Город / локация пользователя (например, "Москва").</summary>
    public string? Location { get; set; }

    /// <summary>Кешированный средний рейтинг пользователя (0..5). Пересчитывается при добавлении Review.</summary>
    public double Rating { get; set; }

    /// <summary>Дата регистрации (UTC). Для сортировок и показа в профиле.</summary>
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    /// <summary>Заблокирован ли пользователь админом (мягкая блокировка, без удаления).</summary>
    public bool IsBlocked { get; set; }

    // ============================================================
    // Навигационные свойства (EF Core поймёт их по соглашению имён).
    // Они НЕ создают колонок в таблице User, это просто связи 1-M / M-M.
    // ============================================================

    public ICollection<Book> Books { get; set; } = new List<Book>();

    /// <summary>Заявки на обмен, отправленные этим пользователем.</summary>
    public ICollection<ExchangeRequest> SentRequests { get; set; } = new List<ExchangeRequest>();

    /// <summary>Заявки на обмен, полученные этим пользователем (как владельцем желаемой книги).</summary>
    public ICollection<ExchangeRequest> ReceivedRequests { get; set; } = new List<ExchangeRequest>();

    /// <summary>Отзывы, оставленные этим пользователем другим участникам.</summary>
    public ICollection<Review> ReviewsGiven { get; set; } = new List<Review>();

    /// <summary>Отзывы, полученные этим пользователем.</summary>
    public ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();

    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();

    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public ICollection<Discussion> Discussions { get; set; } = new List<Discussion>();

    public ICollection<DiscussionMessage> DiscussionMessages { get; set; } = new List<DiscussionMessage>();
}
