using AutoMapper;
using BookExchange.Db.Entities;
using BookExchange.Db.Interfaces;
using BookExchange.Web.Helpers;
using BookExchange.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookExchange.Web.Controllers;

/// <summary>
/// Каталог книг, страница книги, CRUD моих книг.
/// </summary>
public class BookController : Controller
{
    private const int PageSize = 24;

    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _env;

    public BookController(IUnitOfWork uow, IMapper mapper, UserManager<User> um, IWebHostEnvironment env)
    {
        _uow = uow;
        _mapper = mapper;
        _userManager = um;
        _env = env;
    }

    // ------------- Каталог (/Book/Index или /catalog через маршрут ниже) -------------
    [HttpGet]
    [Route("Book")]
    [Route("Book/Index")]
    [Route("catalog")]
    public async Task<IActionResult> Index(
        string? search = null,
        string? genres = null,
        string? conditions = null,
        bool onlyAvailable = false,
        int page = 1)
    {
        page = page < 1 ? 1 : page;

        var q = _uow.Books.Query().Where(b => !b.IsHidden).Include(b => b.Owner).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(b => b.Title.ToLower().Contains(s) || b.Author.ToLower().Contains(s));
        }

        var selectedGenres = genres?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => x.Length > 0).ToList() ?? new();
        if (selectedGenres.Count > 0)
            q = q.Where(b => selectedGenres.Contains(b.Genre));

        var selectedConditions = (conditions?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>())
            .Select(x => Enum.TryParse<BookCondition>(x, out var c) ? (BookCondition?)c : null)
            .Where(x => x != null).Select(x => x!.Value).ToList();
        if (selectedConditions.Count > 0)
            q = q.Where(b => selectedConditions.Contains(b.Condition));

        if (onlyAvailable)
            q = q.Where(b => b.IsAvailable);

        var total = await q.CountAsync();

        var books = await q
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var allGenres = await _uow.Books.Query().Where(b => !b.IsHidden)
            .Select(b => b.Genre).Distinct().OrderBy(g => g).ToListAsync();

        var vm = new BookCatalogViewModel
        {
            Books = books.Select(_mapper.Map<BookCardViewModel>).ToList(),
            Pagination = new PaginationInfo { Page = page, PageSize = PageSize, TotalItems = total },
            SearchQuery = search,
            AllGenres = allGenres,
            SelectedGenres = selectedGenres,
            SelectedConditions = selectedConditions,
            OnlyAvailable = onlyAvailable
        };
        return View(vm);
    }

    // ------------- Страница книги -------------
    [HttpGet]
    [Route("Book/Details/{id:int}")]
    [Route("book/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var book = await _uow.Books.Query()
            .Include(b => b.Owner).ThenInclude(u => u!.Books)
            .Include(b => b.Discussions).ThenInclude(d => d.User)
            .Include(b => b.Discussions).ThenInclude(d => d.Messages)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsHidden);
        if (book == null) return NotFound();

        var similar = await _uow.Books.Query()
            .Where(b => b.Id != id && !b.IsHidden && (b.Genre == book.Genre || b.Author == book.Author))
            .Include(b => b.Owner)
            .Take(4)
            .ToListAsync();

        var currentId = _userManager.GetUserId(User);
        var isFav = currentId != null && await _uow.Favorites.AnyAsync(f => f.UserId == currentId && f.BookId == id);
        var isWish = currentId != null && await _uow.Wishlist.AnyAsync(w => w.UserId == currentId && w.BookId == id);

        var vm = _mapper.Map<BookDetailsViewModel>(book);
        vm.Similar = similar.Select(_mapper.Map<BookCardViewModel>).ToList();
        vm.Discussions = book.Discussions.OrderByDescending(d => d.CreatedAt).Select(d => new DiscussionListItemViewModel
        {
            Id = d.Id,
            Title = d.Title,
            AuthorName = d.User?.UserName ?? "",
            MessagesCount = d.Messages.Count,
            CreatedAt = d.CreatedAt
        }).ToList();
        vm.IsFavorite = isFav;
        vm.IsInWishlist = isWish;

        return View(vm);
    }

    // ------------- CRUD моих книг -------------
    [Authorize, HttpGet]
    public IActionResult Create()
    {
        return View("Form", new BookFormViewModel());
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookFormViewModel model)
    {
        if (!ModelState.IsValid) return View("Form", model);

        var userId = _userManager.GetUserId(User)!;
        var book = _mapper.Map<Book>(model);
        book.OwnerId = userId;

        var path = await ImageHelper.SaveAsync(model.CoverImage, _env, "images/books");
        if (path != null) book.CoverImagePath = path;

        await _uow.Books.AddAsync(book);
        await _uow.SaveChangesAsync();

        TempData["Success"] = "Книга добавлена.";
        return RedirectToAction("Details", "User", new { id = userId });
    }

    [Authorize, HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var book = await _uow.Books.GetByIdAsync(id);
        if (book == null || book.OwnerId != userId) return Forbid();

        var vm = _mapper.Map<BookFormViewModel>(book);
        return View("Form", vm);
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BookFormViewModel model)
    {
        if (!ModelState.IsValid) return View("Form", model);

        var userId = _userManager.GetUserId(User)!;
        if (model.Id == null) return BadRequest();
        var book = await _uow.Books.GetByIdAsync(model.Id.Value);
        if (book == null || book.OwnerId != userId) return Forbid();

        book.Title = model.Title;
        book.Author = model.Author;
        book.ISBN = model.ISBN;
        book.Description = model.Description;
        book.Genre = model.Genre;
        book.Condition = model.Condition;
        book.Year = model.Year;
        book.Language = model.Language;

        var path = await ImageHelper.SaveAsync(model.CoverImage, _env, "images/books");
        if (path != null) book.CoverImagePath = path;

        _uow.Books.Update(book);
        await _uow.SaveChangesAsync();

        TempData["Success"] = "Книга обновлена.";
        return RedirectToAction("Details", new { id = book.Id });
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var book = await _uow.Books.GetByIdAsync(id);
        if (book == null || book.OwnerId != userId) return Forbid();

        _uow.Books.Remove(book);
        await _uow.SaveChangesAsync();
        return RedirectToAction("Details", "User", new { id = userId });
    }

    // ------------- ISBN -> авто-заполнение (AJAX) -------------
    [Authorize, HttpGet]
    public async Task<IActionResult> FetchByIsbn(string isbn)
    {
        var r = await GoogleBooksHelper.FetchByISBNAsync(isbn);
        if (r == null) return Json(new { found = false });
        return Json(new { found = true, r.Title, r.Author, r.Description, Cover = r.CoverImageUrl, r.Year });
    }
}
