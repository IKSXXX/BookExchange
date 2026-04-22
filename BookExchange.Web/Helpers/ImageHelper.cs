using Microsoft.AspNetCore.Http;

namespace BookExchange.Web.Helpers;

/// <summary>
/// Сохранение загружаемых файлов (аватары, обложки) на диск.
/// Пути хранятся в БД как относительные — от wwwroot (например "/images/books/abc.jpg").
/// </summary>
public static class ImageHelper
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long MaxSizeBytes = 5 * 1024 * 1024; // 5 МБ

    /// <summary>
    /// Сохранить файл в <c>wwwroot/{subFolder}/</c> с уникальным именем.
    /// Возвращает относительный URL (например "/images/books/abc.jpg") или null при ошибке.
    /// </summary>
    public static async Task<string?> SaveAsync(IFormFile? file, IWebHostEnvironment env, string subFolder)
    {
        if (file == null || file.Length == 0) return null;
        if (file.Length > MaxSizeBytes) return null;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext)) return null;

        var folder = Path.Combine(env.WebRootPath, subFolder);
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folder, fileName);

        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream);

        // Всегда возвращаем путь через прямые слеши — он идёт в HTML-атрибут src.
        return "/" + subFolder.Replace('\\', '/').Trim('/') + "/" + fileName;
    }
}
