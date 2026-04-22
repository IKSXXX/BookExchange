using BookExchange.Db.Entities;
using BookExchange.Db.Interfaces;
using BookExchange.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookExchange.Web.Controllers;

public class DiscussionController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<User> _userManager;

    public DiscussionController(IUnitOfWork uow, UserManager<User> um)
    {
        _uow = uow;
        _userManager = um;
    }

    [HttpGet("Discussion/Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var d = await _uow.Discussions.Query()
            .Include(x => x.Book)
            .Include(x => x.User)
            .Include(x => x.Messages).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (d == null) return NotFound();

        var vm = new DiscussionDetailsViewModel
        {
            Id = d.Id,
            BookId = d.BookId,
            BookTitle = d.Book?.Title ?? "",
            Title = d.Title,
            AuthorName = d.User?.UserName ?? "",
            Messages = d.Messages.OrderBy(m => m.CreatedAt).Select(m => new DiscussionMessageViewModel
            {
                UserName = m.User?.UserName ?? "",
                AvatarPath = m.User?.AvatarPath,
                Text = m.Text,
                CreatedAt = m.CreatedAt
            }).ToList()
        };
        return View(vm);
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int bookId, string title)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > 300)
        {
            TempData["Error"] = "Введите название темы (до 300 символов).";
            return RedirectToAction("Details", "Book", new { id = bookId });
        }
        var userId = _userManager.GetUserId(User)!;
        var book = await _uow.Books.GetByIdAsync(bookId);
        if (book == null) return NotFound();

        var d = new Discussion
        {
            BookId = bookId,
            UserId = userId,
            Title = title.Trim()
        };
        await _uow.Discussions.AddAsync(d);
        await _uow.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = d.Id });
    }
}
