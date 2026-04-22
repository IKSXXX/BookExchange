using BookExchange.Db.Entities;
using BookExchange.Db.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace BookExchange.Web.Hubs;

/// <summary>SignalR-хаб для тем обсуждений книг.</summary>
[Authorize]
public class DiscussionHub : Hub
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<User> _userManager;

    public DiscussionHub(IUnitOfWork uow, UserManager<User> userManager)
    {
        _uow = uow;
        _userManager = userManager;
    }

    public Task JoinDiscussion(int discussionId)
        => Groups.AddToGroupAsync(Context.ConnectionId, GroupName(discussionId));

    public async Task SendMessage(int discussionId, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var user = await _userManager.GetUserAsync(Context.User!);
        if (user == null) return;

        var message = new DiscussionMessage
        {
            DiscussionId = discussionId,
            UserId = user.Id,
            Text = text.Trim()
        };
        await _uow.DiscussionMessages.AddAsync(message);
        await _uow.SaveChangesAsync();

        await Clients.Group(GroupName(discussionId)).SendAsync("ReceiveMessage", new
        {
            userName = user.UserName,
            avatarPath = user.AvatarPath,
            text = message.Text,
            createdAt = message.CreatedAt
        });
    }

    private static string GroupName(int discussionId) => $"discussion-{discussionId}";
}
