using BookExchange.Db.Entities;
using BookExchange.Db.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BookExchange.Web.Hubs;

/// <summary>
/// SignalR-хаб для чата внутри конкретной заявки на обмен.
/// Каждая заявка = отдельная SignalR-группа с именем "exchange-{id}".
/// Клиент: - вызывает JoinGroup(exchangeId) при открытии страницы,
///         - вызывает SendMessage(exchangeId, text) при отправке сообщения,
///         - подписан на событие "ReceiveMessage" (имя метода на клиенте).
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<User> _userManager;

    public ChatHub(IUnitOfWork uow, UserManager<User> userManager)
    {
        _uow = uow;
        _userManager = userManager;
    }

    public async Task JoinGroup(int exchangeId)
    {
        var user = await _userManager.GetUserAsync(Context.User!);
        if (user == null) return;

        // Разрешаем подключаться только участникам обмена.
        var ex = await _uow.Exchanges.GetByIdAsync(exchangeId);
        if (ex == null || (ex.SenderId != user.Id && ex.ReceiverId != user.Id)) return;

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(exchangeId));
    }

    public async Task SendMessage(int exchangeId, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var user = await _userManager.GetUserAsync(Context.User!);
        if (user == null) return;

        var ex = await _uow.Exchanges.GetByIdAsync(exchangeId);
        if (ex == null || (ex.SenderId != user.Id && ex.ReceiverId != user.Id)) return;

        var message = new Message
        {
            ExchangeRequestId = exchangeId,
            SenderId = user.Id,
            Text = text.Trim(),
            SentAt = DateTime.UtcNow
        };
        await _uow.Messages.AddAsync(message);
        await _uow.SaveChangesAsync();

        // Оповещаем всех в группе (включая отправителя).
        await Clients.Group(GroupName(exchangeId)).SendAsync("ReceiveMessage", new
        {
            senderId = user.Id,
            senderName = user.UserName,
            senderAvatar = user.AvatarPath,
            text = message.Text,
            sentAt = message.SentAt
        });
    }

    private static string GroupName(int exchangeId) => $"exchange-{exchangeId}";
}
