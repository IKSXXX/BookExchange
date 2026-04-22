using AutoMapper;
using BookExchange.Db.Entities;
using BookExchange.Db.Interfaces;
using BookExchange.Web.Helpers;
using BookExchange.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookExchange.Web.Controllers;

/// <summary>
/// Главная страница: карусель (статическая), книга дня, мини-игра, AI-виджет, последние книги.
/// </summary>
public class HomeController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public HomeController(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        var today = DateTime.UtcNow.Date;
        var bod = await _uow.BooksOfTheDay.Query()
            .Where(b => b.Date == today)
            .Include(b => b.Book!).ThenInclude(bk => bk.Owner)
            .FirstOrDefaultAsync();

        var recent = await _uow.Books.Query()
            .Where(b => !b.IsHidden)
            .Include(b => b.Owner)
            .OrderByDescending(b => b.CreatedAt)
            .Take(6)
            .ToListAsync();

        var quiz = await GetRandomQuizAsync();

        var vm = new HomeViewModel
        {
            BookOfTheDay = bod?.Book != null ? _mapper.Map<BookCardViewModel>(bod.Book) : null,
            RecentBooks = recent.Select(_mapper.Map<BookCardViewModel>).ToList(),
            QuizQuestion = quiz
        };
        return View(vm);
    }

    // ------------- Случайная книга -------------
    [HttpGet("/book/random")]
    public async Task<IActionResult> Random()
    {
        var ids = await _uow.Books.Query().Where(b => !b.IsHidden && b.IsAvailable).Select(b => b.Id).ToListAsync();
        if (ids.Count == 0) return RedirectToAction(nameof(Index));
        var id = ids[System.Random.Shared.Next(ids.Count)];
        return RedirectToAction("Details", "Book", new { id });
    }

    // ------------- Мини-игра: получить случайный вопрос (AJAX) -------------
    [HttpGet("/quiz/next")]
    public async Task<IActionResult> QuizNext()
    {
        var q = await GetRandomQuizAsync();
        return Json(q);
    }

    // ------------- Мини-игра: проверить ответ (AJAX) -------------
    [HttpPost("/quiz/answer")]
    public async Task<IActionResult> QuizAnswer([FromForm] int questionId, [FromForm] string answer)
    {
        var q = await _uow.QuizQuestions.GetByIdAsync(questionId);
        if (q == null) return NotFound();

        return Json(new QuizAnswerResultViewModel
        {
            IsCorrect = string.Equals(answer?.Trim(), q.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase),
            CorrectAnswer = q.CorrectAnswer,
            BookId = q.BookId
        });
    }

    // ------------- AI-виджет: получить рекомендации (AJAX) -------------
    [HttpPost("/ai/recommend")]
    public async Task<IActionResult> AiRecommend([FromForm] string? query)
    {
        var books = await _uow.Books.Query()
            .Where(b => !b.IsHidden && b.IsAvailable)
            .Include(b => b.Owner)
            .ToListAsync();

        var result = AIHelper.GetRecommendations(query ?? string.Empty, books)
            .Select(_mapper.Map<BookCardViewModel>)
            .ToList();
        return Json(new AiRecommendationViewModel { Query = query ?? string.Empty, Books = result });
    }

    private async Task<QuizQuestionViewModel?> GetRandomQuizAsync()
    {
        var count = await _uow.QuizQuestions.Query().CountAsync();
        if (count == 0) return null;
        var skip = System.Random.Shared.Next(count);
        var q = await _uow.QuizQuestions.Query().Skip(skip).Take(1).FirstAsync();

        // Перемешаем варианты.
        var opts = new List<string> { q.CorrectAnswer, q.Option2, q.Option3, q.Option4 };
        var rnd = new Random();
        opts = opts.OrderBy(_ => rnd.Next()).ToList();

        return new QuizQuestionViewModel { Id = q.Id, Quote = q.Quote, Options = opts, BookId = q.BookId };
    }

    [HttpGet("/privacy")]
    public IActionResult Privacy() => View();

    [HttpGet("/error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
