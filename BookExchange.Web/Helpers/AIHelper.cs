using BookExchange.Db.Entities;

namespace BookExchange.Web.Helpers;

/// <summary>
/// Заглушка "AI-рекомендаций". Вместо похода в OpenAI/Gemini просто ищем ключевые слова запроса
/// в Title/Author/Description/Genre. Это симулирует поведение: пользователь вводит "фэнтези про магию",
/// мы возвращаем подходящие книги.
/// </summary>
public static class AIHelper
{
    public static IEnumerable<Book> GetRecommendations(string userQuery, IEnumerable<Book> availableBooks, int take = 6)
    {
        if (string.IsNullOrWhiteSpace(userQuery)) return availableBooks.Take(take);

        // TODO (реальная реализация): вызвать LLM API (OpenAI/Gemini) с промптом + списком книг,
        // получить top-K идентификаторов, вернуть соответствующие книги.
        var tokens = userQuery
            .ToLowerInvariant()
            .Split(new[] { ' ', ',', '.', ';', '!', '?', '-' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t.Length > 2)
            .ToArray();

        if (tokens.Length == 0) return availableBooks.Take(take);

        return availableBooks
            .Select(b => new { b, score = Score(b, tokens) })
            .Where(x => x.score > 0)
            .OrderByDescending(x => x.score)
            .Take(take)
            .Select(x => x.b);
    }

    private static int Score(Book b, string[] tokens)
    {
        int score = 0;
        var haystack = string.Join(" ", new[] { b.Title, b.Author, b.Description, b.Genre }).ToLowerInvariant();
        foreach (var t in tokens)
        {
            if (haystack.Contains(t)) score++;
        }
        return score;
    }
}
