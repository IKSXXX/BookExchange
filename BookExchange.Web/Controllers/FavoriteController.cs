using BookExchange.Db.Entities;
using BookExchange.Db.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookExchange.Web.Controllers;

[Authorize]
public class FavoriteController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<User> _userManager;

    public FavoriteController(IUnitOfWork uow, UserManager<User> um)
    {
        _uow = uow;
        _userManager = um;
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int bookId, string? returnUrl = null)
    {
        var userId = _userManager.GetUserId(User)!;
        var existing = (await _uow.Favorites.FindAsync(f => f.UserId == userId && f.BookId == bookId)).FirstOrDefault();

        if (existing != null)
            _uow.Favorites.Remove(existing);
        else
            await _uow.Favorites.AddAsync(new Favorite { UserId = userId, BookId = bookId });

        await _uow.SaveChangesAsync();
        return Redirect(returnUrl ?? Url.Action("Details", "Book", new { id = bookId })!);
    }
}
