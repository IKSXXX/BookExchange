using System.ComponentModel.DataAnnotations;
using BookExchange.Db.Entities;

namespace BookExchange.Web.ViewModels;

/// <summary>Лёгкая модель для карточки книги в каталоге / на главной.</summary>
public class BookCardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? CoverImagePath { get; set; }
    public string Genre { get; set; } = string.Empty;
    public string ConditionLabel { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string? Description { get; set; }
    public OwnerSummaryViewModel Owner { get; set; } = new();
}

public class OwnerSummaryViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public int BooksCount { get; set; }
    public int ExchangesCount { get; set; }
    public double Rating { get; set; }
}

/// <summary>Детальный просмотр книги.</summary>
public class BookDetailsViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? ISBN { get; set; }
    public string? Description { get; set; }
    public string? CoverImagePath { get; set; }
    public string Genre { get; set; } = string.Empty;
    public BookCondition Condition { get; set; }
    public string ConditionLabel { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string? Language { get; set; }
    public bool IsAvailable { get; set; }
    public OwnerSummaryViewModel Owner { get; set; } = new();
    public List<BookCardViewModel> Similar { get; set; } = new();
    public List<DiscussionListItemViewModel> Discussions { get; set; } = new();
    public bool IsFavorite { get; set; }
    public bool IsInWishlist { get; set; }
}

public class DiscussionListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int MessagesCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DiscussionDetailsViewModel
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public List<DiscussionMessageViewModel> Messages { get; set; } = new();
}

public class DiscussionMessageViewModel
{
    public string UserName { get; set; } = string.Empty;
    public string? AvatarPath { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>Пагинация для каталога.</summary>
public class PaginationInfo
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public bool HasPrev => Page > 1;
    public bool HasNext => Page < TotalPages;
}

/// <summary>Полный view-model страницы каталога.</summary>
public class BookCatalogViewModel
{
    public List<BookCardViewModel> Books { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();

    // Фильтры / поиск
    public string? SearchQuery { get; set; }
    public List<string> AllGenres { get; set; } = new();
    public List<string> SelectedGenres { get; set; } = new();
    public List<BookCondition> SelectedConditions { get; set; } = new();
    public bool OnlyAvailable { get; set; }

    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(SearchQuery)
        || SelectedGenres.Count > 0
        || SelectedConditions.Count > 0
        || OnlyAvailable;
}

/// <summary>Форма создания/редактирования книги (в профиле).</summary>
public class BookFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Введите название")]
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите автора")]
    [StringLength(200)]
    public string Author { get; set; } = string.Empty;

    [StringLength(32)]
    public string? ISBN { get; set; }

    [StringLength(4000)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string Genre { get; set; } = "Другое";

    public BookCondition Condition { get; set; } = BookCondition.Good;

    [Range(1500, 2100)]
    public int? Year { get; set; }

    [StringLength(60)]
    public string? Language { get; set; } = "Русский";

    /// <summary>Файл обложки (optional).</summary>
    public Microsoft.AspNetCore.Http.IFormFile? CoverImage { get; set; }

    /// <summary>Уже сохранённый путь к обложке (для редактирования).</summary>
    public string? ExistingCoverPath { get; set; }
}
