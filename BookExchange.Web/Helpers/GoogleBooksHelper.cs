using BookExchange.Db.Entities;

namespace BookExchange.Web.Helpers;

/// <summary>
/// Заглушка Google Books API. По ISBN возвращает мок-данные.
/// Реальная интеграция: HTTPS-запрос на
/// <c>https://www.googleapis.com/books/v1/volumes?q=isbn:{isbn}</c> и парсинг JSON.
/// Здесь же мы просто возвращаем словарную запись для демонстрации.
/// </summary>
public static class GoogleBooksHelper
{
    public record GoogleBookResult(string Title, string Author, string? Description, string? CoverImageUrl, int? Year);

    private static readonly Dictionary<string, GoogleBookResult> Mock = new()
    {
        ["9785170908691"] = new("1984", "Джордж Оруэлл", "Классическая антиутопия о тоталитарном обществе.",
            "https://images.unsplash.com/photo-1543002588-bfa74002ed7e?w=400&h=600&fit=crop", 1949),
        ["9785699801234"] = new("Мастер и Маргарита", "Михаил Булгаков", "Мистический роман о любви и дьяволе в Москве.",
            "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400&h=600&fit=crop", 1967)
    };

    public static Task<GoogleBookResult?> FetchByISBNAsync(string? isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn)) return Task.FromResult<GoogleBookResult?>(null);
        // TODO (реальная реализация): HttpClient + GET на Google Books API, десериализация JSON.
        return Task.FromResult(Mock.TryGetValue(isbn.Trim(), out var r) ? r : null);
    }
}
