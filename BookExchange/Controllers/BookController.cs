using Microsoft.AspNetCore.Mvc;
using BookExchange.ViewModels;
using BookExchange.Data;

namespace BookExchange.Controllers
{
    public class BookController : Controller
    {
        public IActionResult Index(string search = "",
                           List<string>? genreIds = null,
                           List<string>? conditionNames = null,
                           bool showOnlyAvailable = false,
                           int page = 1)
        {
            const int pageSize = 12;
            var allBooks = MockData.GetMockBooks();

            // Фильтрация
            var filtered = allBooks.AsEnumerable();

            if (!string.IsNullOrEmpty(search))
                filtered = filtered.Where(b => b.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                                b.Author.Contains(search, StringComparison.OrdinalIgnoreCase));

            if (genreIds != null && genreIds.Any())
                filtered = filtered.Where(b => genreIds.Contains(b.Genre));

            if (conditionNames != null && conditionNames.Any())
                filtered = filtered.Where(b => conditionNames.Contains(b.Condition));

            if (showOnlyAvailable)
                filtered = filtered.Where(b => b.IsAvailable);

            var totalItems = filtered.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var books = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var model = new BookCatalogViewModel
            {
                Books = books,
                AllGenres = new List<string> { "Классика", "Фэнтези", "Антиутопия", "Научная литература", "Философия", "Детская литература", "Современная проза", "Фантастика" },
                SelectedGenres = genreIds ?? new List<string>(),
                AllConditions = new List<string> { "новая", "отличное", "хорошее", "удовлетворительное" },
                SelectedConditions = conditionNames ?? new List<string>(),
                ShowOnlyAvailable = showOnlyAvailable,
                SearchQuery = search,
                Pagination = new PaginationInfo
                {
                    CurrentPage = page,
                    TotalPages = totalPages,
                    TotalItems = totalItems,
                    PageSize = pageSize
                }
            };
            return View(model);
        }
    }
}