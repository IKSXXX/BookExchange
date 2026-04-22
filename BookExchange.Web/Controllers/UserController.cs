using AutoMapper;
using BookExchange.Db.Entities;
using BookExchange.Db.Interfaces;
using BookExchange.Web.Helpers;
using BookExchange.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookExchange.Web.Controllers;

/// <summary>Публичная страница профиля пользователя с вкладками.</summary>
public class UserController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UserController(UserManager<User> um, IUnitOfWork uow, IMapper mapper)
    {
        _userManager = um;
        _uow = uow;
        _mapper = mapper;
    }

    [HttpGet]
    [Route("User/{id}")]
    public async Task<IActionResult> Details(string id)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();

        var currentId = _userManager.GetUserId(User);
        var isCurrent = currentId == user.Id;

        var myBooks = await _uow.Books.Query()
            .Where(b => b.OwnerId == user.Id && !b.IsHidden)
            .Include(b => b.Owner)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        var exchanges = await _uow.Exchanges.Query()
            .Where(e => e.SenderId == user.Id || e.ReceiverId == user.Id)
            .Include(e => e.Sender).Include(e => e.Receiver)
            .Include(e => e.BookRequested).Include(e => e.BookOffered)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        var favorites = await _uow.Favorites.Query()
            .Where(f => f.UserId == user.Id)
            .Include(f => f.Book!).ThenInclude(b => b.Owner)
            .Select(f => f.Book!)
            .ToListAsync();

        var wishlist = await _uow.Wishlist.Query()
            .Where(w => w.UserId == user.Id)
            .Include(w => w.Book!).ThenInclude(b => b.Owner)
            .Select(w => w.Book!)
            .ToListAsync();

        var reviewsReceived = await _uow.Reviews.Query()
            .Where(r => r.ToUserId == user.Id)
            .Include(r => r.FromUser).Include(r => r.ToUser)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var reviewsGiven = await _uow.Reviews.Query()
            .Where(r => r.FromUserId == user.Id)
            .Include(r => r.FromUser).Include(r => r.ToUser)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var vm = new UserProfileViewModel
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = isCurrent ? user.Email : null,
            AvatarPath = user.AvatarPath,
            Location = user.Location,
            Rating = user.Rating,
            RegistrationDate = user.RegistrationDate,
            BooksCount = myBooks.Count,
            CompletedExchanges = exchanges.Count(e => e.Status == ExchangeStatus.Completed),
            MyBooks = myBooks.Select(_mapper.Map<BookCardViewModel>).ToList(),
            Exchanges = exchanges.Select(e => new ExchangeListItemViewModel
            {
                Id = e.Id,
                Status = e.Status,
                StatusLabel = MappingProfile.StatusToLabel(e.Status),
                CreatedAt = e.CreatedAt,
                IsSender = e.SenderId == user.Id,
                OtherUserName = (e.SenderId == user.Id ? e.Receiver?.UserName : e.Sender?.UserName) ?? "",
                OtherUserAvatar = (e.SenderId == user.Id ? e.Receiver?.AvatarPath : e.Sender?.AvatarPath) ?? "",
                BookRequestedTitle = e.BookRequested?.Title ?? "",
                BookOfferedTitle = e.BookOffered?.Title
            }).ToList(),
            Favorites = favorites.Select(_mapper.Map<BookCardViewModel>).ToList(),
            Wishlist = wishlist.Select(_mapper.Map<BookCardViewModel>).ToList(),
            ReviewsReceived = reviewsReceived.Select(r => new ReviewDisplayViewModel
            {
                FromUserName = r.FromUser?.UserName ?? "",
                FromUserAvatar = r.FromUser?.AvatarPath,
                ToUserName = r.ToUser?.UserName ?? "",
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList(),
            ReviewsGiven = reviewsGiven.Select(r => new ReviewDisplayViewModel
            {
                FromUserName = r.FromUser?.UserName ?? "",
                FromUserAvatar = r.FromUser?.AvatarPath,
                ToUserName = r.ToUser?.UserName ?? "",
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList(),
            IsCurrentUser = isCurrent
        };

        return View(vm);
    }
}
