using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Shared.Data.TypedIds;
using Shared.Extensions;
using Shared.Models;
using Shared.Signalr.Clients;
using Squadtalk.Data.Entities;
using Squadtalk.Data.Repositories;
using Squadtalk.Extensions;
using Squadtalk.Services;

namespace Squadtalk.Signalr;

public interface IChatClient : ITextChatClient, IVoiceChatClient, IFriendChatClient;

[Authorize]
public partial class AppHub : Hub<IChatClient>
{
    private readonly HubConnectionManager _connectionManager;
    private readonly ChatUserRepository _userRepository;
    private readonly FriendRepository _friendRepository;
    private readonly ILogger<AppHub> _logger;

    public AppHub(
        HubConnectionManager connectionManager,
        ChatUserRepository userRepository,
        FriendRepository friendRepository,
        ILogger<AppHub> logger)
    {
        _connectionManager = connectionManager;
        _userRepository = userRepository;
        _friendRepository = friendRepository;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var user = await _userRepository.FindUserByIdAsync(Context.User, GroupInclusionOption.Include);
        if (user is null)
        {
            Context.Abort();
            return;
        }

        var (statusChanged, currentStatus) = await _connectionManager.ConnectionStartedAsync(user.Id, Context.ConnectionId);

        await Groups.AddToGroupAsync(Context.ConnectionId, ChatModel.GlobalChatId);

        if (user.GroupParticipants is { Count: > 0 })
        {
            var tasks = user.Groups
                .Select(x => Groups.AddToGroupAsync(Context.ConnectionId, x.Id));

            await Task.WhenAll(tasks);
        }

        if (statusChanged)
        {
            var friendsId = await _friendRepository.GetUserFriendIdsAsync(user.Id);
            var stringIds = friendsId.Select(x => x.ToString());

            await Clients.Users(stringIds).FriendStatusChanged(user.Id, currentStatus);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = await _userRepository.FindUserByIdAsync(Context.User);
        if (user is null)
        {
            return;
        }

        var (statusChanged, currentStatus) = await _connectionManager.ConnectionClosedAsync(user.Id, Context.ConnectionId);

        if (statusChanged)
        {
            var friendsId = await _friendRepository.GetUserFriendIdsAsync(user.Id);
            var stringIds = friendsId.Select(x => x.ToString());

            await Clients.Users(stringIds).FriendStatusChanged(user.Id, currentStatus);
        }
    }

    private async Task<ChatUser?> GetParticipatingUserWithGroups(GroupId groupId)
    {
        var user = await _userRepository.FindUserByIdAsync(Context.User, GroupInclusionOption.Include);
        if (user is null || !user.ParticipatesInChannel(groupId))
        {
            return null;
        }

        return user;
    }

    private Task<bool> UserParticipatesInChannelAsync(GroupId groupId)
    {
        return groupId == ChatModel.GlobalChatId
            ? Task.FromResult(true)
            : _userRepository.UserParticipatesInGroupAsync(UserId, groupId);
    }

    private UserId UserId => Context.User!.GetRequiredUserId();
}
