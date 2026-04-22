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

[Authorize]
public class ExchangeController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;

    public ExchangeController(IUnitOfWork uow, IMapper mapper, UserManager<User> um)
    {
        _uow = uow;
        _mapper = mapper;
        _userManager = um;
    }

    // ------------- Создание заявки -------------
    [HttpGet("Exchange/Create/{bookId:int}")]
    public async Task<IActionResult> Create(int bookId)
    {
        var book = await _uow.Books.Query().Include(b => b.Owner).FirstOrDefaultAsync(b => b.Id == bookId);
        if (book == null || !book.IsAvailable) return NotFound();

        var userId = _userManager.GetUserId(User)!;
        if (book.OwnerId == userId) return BadRequest("Нельзя обменяться с самим собой.");

        var myBooks = await _uow.Books.Query()
            .Where(b => b.OwnerId == userId && b.IsAvailable && !b.IsHidden)
            .Include(b => b.Owner)
            .ToListAsync();

        var vm = new CreateExchangeViewModel
        {
            BookRequestedId = book.Id,
            BookRequested = _mapper.Map<BookCardViewModel>(book),
            MyAvailableBooks = myBooks.Select(_mapper.Map<BookCardViewModel>).ToList()
        };
        return View(vm);
    }

    [HttpPost("Exchange/Create/{bookId:int}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int bookId, int? selectedOfferedBookId)
    {
        var book = await _uow.Books.GetByIdAsync(bookId);
        if (book == null || !book.IsAvailable) return NotFound();

        var userId = _userManager.GetUserId(User)!;
        if (book.OwnerId == userId) return BadRequest();

        // Если предлагают книгу — проверим, что она принадлежит текущему пользователю и доступна.
        Book? offered = null;
        if (selectedOfferedBookId.HasValue)
        {
            offered = await _uow.Books.GetByIdAsync(selectedOfferedBookId.Value);
            if (offered == null || offered.OwnerId != userId || !offered.IsAvailable) return BadRequest();
        }

        var request = new ExchangeRequest
        {
            BookRequestedId = book.Id,
            BookOfferedId = offered?.Id,
            SenderId = userId,
            ReceiverId = book.OwnerId,
            Status = ExchangeStatus.Pending
        };
        await _uow.Exchanges.AddAsync(request);
        await _uow.SaveChangesAsync();

        TempData["Success"] = "Заявка отправлена!";
        return RedirectToAction(nameof(Details), new { id = request.Id });
    }

    // ------------- Детали заявки + чат -------------
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var ex = await _uow.Exchanges.Query()
            .Include(e => e.Sender).Include(e => e.Receiver)
            .Include(e => e.BookRequested!).ThenInclude(b => b.Owner)
            .Include(e => e.BookOffered!).ThenInclude(b => b.Owner)
            .Include(e => e.Messages).ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (ex == null) return NotFound();

        var userId = _userManager.GetUserId(User)!;
        if (ex.SenderId != userId && ex.ReceiverId != userId) return Forbid();

        var vm = new ExchangeDetailsViewModel
        {
            Id = ex.Id,
            Status = ex.Status,
            StatusLabel = MappingProfile.StatusToLabel(ex.Status),
            CreatedAt = ex.CreatedAt,
            Sender = _mapper.Map<OwnerSummaryViewModel>(ex.Sender),
            Receiver = _mapper.Map<OwnerSummaryViewModel>(ex.Receiver),
            BookRequested = _mapper.Map<BookCardViewModel>(ex.BookRequested!),
            BookOffered = ex.BookOffered != null ? _mapper.Map<BookCardViewModel>(ex.BookOffered) : null,
            Messages = ex.Messages.OrderBy(m => m.SentAt).Select(m => new ChatMessageViewModel
            {
                SenderId = m.SenderId,
                SenderName = m.Sender?.UserName ?? "",
                SenderAvatar = m.Sender?.AvatarPath,
                Text = m.Text,
                SentAt = m.SentAt
            }).ToList(),
            CurrentUserId = userId
        };

        // Логика доступных действий:
        vm.CanAccept = ex.Status == ExchangeStatus.Pending && ex.ReceiverId == userId;
        vm.CanReject = ex.Status == ExchangeStatus.Pending && ex.ReceiverId == userId;
        vm.CanCancel = ex.Status == ExchangeStatus.Pending && ex.SenderId == userId;
        vm.CanComplete = ex.Status == ExchangeStatus.Accepted && (ex.SenderId == userId || ex.ReceiverId == userId);

        // Можно оставить отзыв только если завершено и пользователь ещё не оставлял в рамках этой сделки.
        var alreadyReviewed = await _uow.Reviews.AnyAsync(r => r.ExchangeRequestId == ex.Id && r.FromUserId == userId);
        vm.CanLeaveReview = ex.Status == ExchangeStatus.Completed && !alreadyReviewed;

        return View(vm);
    }

    // ------------- Действия: Accept/Reject/Cancel/Complete -------------
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int id) => await TransitionAsync(id, ExchangeStatus.Pending, ExchangeStatus.Accepted, receiverOnly: true, onAccept: async ex =>
    {
        // Принять — помечаем обе книги как недоступные (идёт обмен).
        var reqBook = await _uow.Books.GetByIdAsync(ex.BookRequestedId);
        if (reqBook != null) { reqBook.IsAvailable = false; _uow.Books.Update(reqBook); }
        if (ex.BookOfferedId.HasValue)
        {
            var offBook = await _uow.Books.GetByIdAsync(ex.BookOfferedId.Value);
            if (offBook != null) { offBook.IsAvailable = false; _uow.Books.Update(offBook); }
        }
    });

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Reject(int id)
        => TransitionAsync(id, ExchangeStatus.Pending, ExchangeStatus.Rejected, receiverOnly: true);

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Cancel(int id)
        => TransitionAsync(id, ExchangeStatus.Pending, ExchangeStatus.Cancelled, senderOnly: true);

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id) => await TransitionAsync(id, ExchangeStatus.Accepted, ExchangeStatus.Completed, onAccept: async ex =>
    {
        // Завершили — книги уходят к новым владельцам.
        var reqBook = await _uow.Books.GetByIdAsync(ex.BookRequestedId);
        if (reqBook != null)
        {
            reqBook.OwnerId = ex.SenderId;
            reqBook.IsAvailable = true;
            _uow.Books.Update(reqBook);
        }
        if (ex.BookOfferedId.HasValue)
        {
            var offBook = await _uow.Books.GetByIdAsync(ex.BookOfferedId.Value);
            if (offBook != null)
            {
                offBook.OwnerId = ex.ReceiverId;
                offBook.IsAvailable = true;
                _uow.Books.Update(offBook);
            }
        }
    });

    private async Task<IActionResult> TransitionAsync(
        int id,
        ExchangeStatus from,
        ExchangeStatus to,
        bool receiverOnly = false,
        bool senderOnly = false,
        Func<ExchangeRequest, Task>? onAccept = null)
    {
        var ex = await _uow.Exchanges.GetByIdAsync(id);
        if (ex == null) return NotFound();

        var userId = _userManager.GetUserId(User)!;
        if (ex.SenderId != userId && ex.ReceiverId != userId) return Forbid();
        if (receiverOnly && ex.ReceiverId != userId) return Forbid();
        if (senderOnly && ex.SenderId != userId) return Forbid();

        if (ex.Status != from)
        {
            TempData["Error"] = "Действие недоступно для текущего статуса.";
            return RedirectToAction(nameof(Details), new { id });
        }

        ex.Status = to;
        _uow.Exchanges.Update(ex);
        if (onAccept != null) await onAccept(ex);
        await _uow.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id });
    }

    // ------------- Отзыв после завершения -------------
    [HttpGet]
    public async Task<IActionResult> LeaveReview(int id)
    {
        var ex = await _uow.Exchanges.Query()
            .Include(e => e.Sender).Include(e => e.Receiver)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (ex == null || ex.Status != ExchangeStatus.Completed) return NotFound();

        var userId = _userManager.GetUserId(User)!;
        if (ex.SenderId != userId && ex.ReceiverId != userId) return Forbid();

        if (await _uow.Reviews.AnyAsync(r => r.ExchangeRequestId == id && r.FromUserId == userId))
        {
            TempData["Error"] = "Отзыв уже оставлен.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var otherId = ex.SenderId == userId ? ex.ReceiverId : ex.SenderId;
        var otherName = (ex.SenderId == userId ? ex.Receiver?.UserName : ex.Sender?.UserName) ?? "";

        return View(new ReviewFormViewModel
        {
            ExchangeRequestId = id,
            ToUserId = otherId,
            ToUserName = otherName
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> LeaveReview(ReviewFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = _userManager.GetUserId(User)!;
        var ex = await _uow.Exchanges.GetByIdAsync(model.ExchangeRequestId);
        if (ex == null || ex.Status != ExchangeStatus.Completed) return NotFound();
        if (ex.SenderId != userId && ex.ReceiverId != userId) return Forbid();
        if (await _uow.Reviews.AnyAsync(r => r.ExchangeRequestId == ex.Id && r.FromUserId == userId)) return BadRequest();

        await _uow.Reviews.AddAsync(new Review
        {
            FromUserId = userId,
            ToUserId = model.ToUserId,
            ExchangeRequestId = ex.Id,
            Rating = model.Rating,
            Comment = model.Comment
        });
        await _uow.SaveChangesAsync();

        // Пересчёт рейтинга у получателя отзыва.
        var avg = await _uow.Reviews.Query().Where(r => r.ToUserId == model.ToUserId).Select(r => r.Rating).ToListAsync();
        var user = await _userManager.FindByIdAsync(model.ToUserId);
        if (user != null)
        {
            user.Rating = avg.Count > 0 ? Math.Round(avg.Average(), 2) : 0;
            await _userManager.UpdateAsync(user);
        }

        TempData["Success"] = "Спасибо за отзыв!";
        return RedirectToAction(nameof(Details), new { id = ex.Id });
    }
}
