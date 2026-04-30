using AutoMapper;
using BookExchange.Db.Entities;
using BookExchange.Db.Interfaces;
using BookExchange.Web.Data;
using BookExchange.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookExchange.Web.Controllers;

[Authorize(Roles = DbSeeder.AdminRole)]
public class AdminController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;

    public AdminController(IUnitOfWork uow, IMapper mapper, UserManager<User> um)
    {
        _uow = uow;
        _mapper = mapper;
        _userManager = um;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new AdminStatsViewModel
        {
            UsersCount = _userManager.Users.Count(),
            BooksCount = await _uow.Books.Query().CountAsync(),
            ExchangesCount = await _uow.Exchanges.Query().CountAsync(),
            ReviewsCount = await _uow.Reviews.Query().CountAsync(),
            CompletedExchanges = await _uow.Exchanges.Query().CountAsync(e => e.Status == ExchangeStatus.Completed),
            PendingExchanges = await _uow.Exchanges.Query().CountAsync(e => e.Status == ExchangeStatus.Pending)
        };
        return View(vm);
    }

    // ----- Пользователи -----
    public async Task<IActionResult> Users()
    {
        var users = _userManager.Users.ToList();
        var list = new List<AdminUserListItemViewModel>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            list.Add(new AdminUserListItemViewModel
            {
                Id = u.Id,
                UserName = u.UserName ?? "",
                Email = u.Email,
                IsBlocked = u.IsBlocked,
                IsAdmin = roles.Contains(DbSeeder.AdminRole),
                Rating = u.Rating,
                RegistrationDate = u.RegistrationDate,
                BooksCount = await _uow.Books.Query().CountAsync(b => b.OwnerId == u.Id)
            });
        }
        return View(list);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBlock(string id)
    {
        var u = await _userManager.FindByIdAsync(id);
        if (u == null) return NotFound();
        u.IsBlocked = !u.IsBlocked;
        await _userManager.UpdateAsync(u);
        return RedirectToAction(nameof(Users));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAdmin(string id)
    {
        var u = await _userManager.FindByIdAsync(id);
        if (u == null) return NotFound();
        if (await _userManager.IsInRoleAsync(u, DbSeeder.AdminRole))
            await _userManager.RemoveFromRoleAsync(u, DbSeeder.AdminRole);
        else
            await _userManager.AddToRoleAsync(u, DbSeeder.AdminRole);
        return RedirectToAction(nameof(Users));
    }

    // ----- Книги (модерация) -----
    public async Task<IActionResult> Books()
    {
        var books = await _uow.Books.Query().Include(b => b.Owner).OrderByDescending(b => b.CreatedAt).ToListAsync();
        return View(books);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBookHidden(int id)
    {
        var b = await _uow.Books.GetByIdAsync(id);
        if (b == null) return NotFound();
        b.IsHidden = !b.IsHidden;
        _uow.Books.Update(b);
        await _uow.SaveChangesAsync();
        return RedirectToAction(nameof(Books));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var b = await _uow.Books.GetByIdAsync(id);
        if (b == null) return NotFound();
        _uow.Books.Remove(b);
        await _uow.SaveChangesAsync();
        return RedirectToAction(nameof(Books));
    }

    // ----- Книга дня -----
    [HttpGet]
    public async Task<IActionResult> BookOfTheDay()
    {
        var today = DateTime.UtcNow.Date;
        var existing = (await _uow.BooksOfTheDay.FindAsync(b => b.Date == today)).FirstOrDefault();
        var books = await _uow.Books.Query().Include(b => b.Owner).Where(b => !b.IsHidden).ToListAsync();
        var vm = new SetBookOfDayViewModel
        {
            Date = today,
            CurrentBookOfDayId = existing?.BookId,
            AvailableBooks = books.Select(_mapper.Map<BookCardViewModel>).ToList()
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> BookOfTheDay(int bookId, DateTime date)
    {
        // Postgres timestamptz требует Kind=Utc — иначе Npgsql кидает исключение.
        var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var existing = (await _uow.BooksOfTheDay.FindAsync(b => b.Date == utcDate)).FirstOrDefault();
        if (existing != null)
        {
            existing.BookId = bookId;
            _uow.BooksOfTheDay.Update(existing);
        }
        else
        {
            await _uow.BooksOfTheDay.AddAsync(new BookOfTheDay { BookId = bookId, Date = utcDate });
        }
        await _uow.SaveChangesAsync();
        TempData["Success"] = "Книга дня обновлена.";
        return RedirectToAction(nameof(BookOfTheDay));
    }

    // ----- Quiz CRUD -----
    public async Task<IActionResult> Quiz()
    {
        var qs = await _uow.QuizQuestions.Query().Include(q => q.Book).OrderBy(q => q.Id).ToListAsync();
        return View(qs);
    }

    [HttpGet]
    public async Task<IActionResult> CreateQuiz()
    {
        var books = await _uow.Books.Query().Include(b => b.Owner).Where(b => !b.IsHidden).ToListAsync();
        return View("QuizForm", new QuizQuestionFormViewModel
        {
            AvailableBooks = books.Select(_mapper.Map<BookCardViewModel>).ToList()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQuiz(QuizQuestionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var books = await _uow.Books.Query().Include(b => b.Owner).Where(b => !b.IsHidden).ToListAsync();
            model.AvailableBooks = books.Select(_mapper.Map<BookCardViewModel>).ToList();
            return View("QuizForm", model);
        }
        var q = new QuizQuestion
        {
            BookId = model.BookId,
            Quote = model.Quote,
            CorrectAnswer = model.CorrectAnswer,
            Option2 = model.Option2,
            Option3 = model.Option3,
            Option4 = model.Option4
        };
        await _uow.QuizQuestions.AddAsync(q);
        await _uow.SaveChangesAsync();
        return RedirectToAction(nameof(Quiz));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuiz(int id)
    {
        var q = await _uow.QuizQuestions.GetByIdAsync(id);
        if (q == null) return NotFound();
        _uow.QuizQuestions.Remove(q);
        await _uow.SaveChangesAsync();
        return RedirectToAction(nameof(Quiz));
    }
}
