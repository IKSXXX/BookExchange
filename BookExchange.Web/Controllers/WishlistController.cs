using BookExchange.Db.Entities;
using BookExchange.Db.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookExchange.Web.Controllers;

[Authorize]
public class WishlistController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<User> _userManager;

    public WishlistController(IUnitOfWork uow, UserManager<User> um)
    {
        _uow = uow;
        _userManager = um;
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int bookId, string? returnUrl = null)
    {
        var userId = _userManager.GetUserId(User)!;
        var existing = (await _uow.Wishlist.FindAsync(w => w.UserId == userId && w.BookId == bookId)).FirstOrDefault();

        if (existing != null)
            _uow.Wishlist.Remove(existing);
        else
            await _uow.Wishlist.AddAsync(new WishlistItem { UserId = userId, BookId = bookId });

        await _uow.SaveChangesAsync();
        return Redirect(returnUrl ?? Url.Action("Details", "Book", new { id = bookId })!);
    }
}
