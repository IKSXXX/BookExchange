using System.ComponentModel.DataAnnotations;

namespace BookExchange.Web.ViewModels;

public class RegisterViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(60, MinimumLength = 3)]
    [Display(Name = "Имя пользователя")]
    public string UserName { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
    [Display(Name = "Повторите пароль")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Запомнить меня")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class EditProfileViewModel
{
    [Required, StringLength(60, MinimumLength = 3)]
    [Display(Name = "Имя пользователя")]
    public string UserName { get; set; } = string.Empty;

    [StringLength(120)]
    [Display(Name = "Город")]
    public string? Location { get; set; }

    [Display(Name = "Новый аватар")]
    public Microsoft.AspNetCore.Http.IFormFile? Avatar { get; set; }

    public string? ExistingAvatarPath { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Новый пароль (необязательно)")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Текущий пароль (обязательно при смене пароля)")]
    public string? CurrentPassword { get; set; }
}

public class UserProfileViewModel
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? AvatarPath { get; set; }
    public string? Location { get; set; }
    public double Rating { get; set; }
    public DateTime RegistrationDate { get; set; }
    public int BooksCount { get; set; }
    public int CompletedExchanges { get; set; }

    public List<BookCardViewModel> MyBooks { get; set; } = new();
    public List<ExchangeListItemViewModel> Exchanges { get; set; } = new();
    public List<BookCardViewModel> Favorites { get; set; } = new();
    public List<BookCardViewModel> Wishlist { get; set; } = new();
    public List<ReviewDisplayViewModel> ReviewsReceived { get; set; } = new();
    public List<ReviewDisplayViewModel> ReviewsGiven { get; set; } = new();

    public bool IsCurrentUser { get; set; }
}

public class ReviewDisplayViewModel
{
    public string FromUserName { get; set; } = string.Empty;
    public string? FromUserAvatar { get; set; }
    public string ToUserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
