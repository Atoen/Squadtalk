using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Shared.Data.TypedIds;
using Shared.Extensions;
using Shared.Models;
using Shared.Signalr.Clients;
using Squadtalk.Data.Entities;
using Squadtalk.Extensions;
using Squadtalk.Repositories;
using Squadtalk.Services;

namespace Squadtalk.Signalr;

public interface IChatClient : ITextChatClient, IVoiceChatClient, IFriendChatClient;

[Authorize]
public partial class AppHub : Hub<IChatClient>
{
    private readonly HubConnectionManager _connectionManager;
    private readonly UserRepository _userRepository;
    private readonly FriendRepository _friendRepository;
    private readonly ILogger<AppHub> _logger;

    public AppHub(
        HubConnectionManager connectionManager,
        UserRepository userRepository,
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
        var user = await _userRepository.FindUserById(Context.User, ChannelsInclusionOption.Include);
        if (user is null)
        {
            Context.Abort();
            return;
        }

        var (statusChanged, currentStatus) = await _connectionManager.ConnectionStartedAsync(user, Context.ConnectionId);
        _logger.LogInformation("User {Username} status changed: {Changed}, now: {Current}", user.UserName, statusChanged, currentStatus);

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupChatModel.GlobalChatId);

        if (user.Channels is { Count: > 0 })
        {
            var tasks = user.Channels
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
        var user = await _userRepository.FindUserById(Context.User);
        if (user is null)
        {
            return;
        }

        var (statusChanged, currentStatus) = await _connectionManager.ConnectionClosedAsync(user, Context.ConnectionId);
        _logger.LogInformation("User {Username} status changed: {Changed}, now: {Current}", user.UserName, statusChanged, currentStatus);

        if (statusChanged)
        {
            var friendsId = await _friendRepository.GetUserFriendIdsAsync(user.Id);
            var stringIds = friendsId.Select(x => x.ToString());

            await Clients.Users(stringIds).FriendStatusChanged(user.Id, currentStatus);
        }
    }

    private async Task<ApplicationUser?> GetChannelParticipantAsync(ChannelId channelId)
    {
        var user = await _userRepository.FindUserById(Context.User, ChannelsInclusionOption.Include);
        if (user is null || !user.ParticipatesInChannel(channelId))
        {
            return null;
        }

        return user;
    }

    private UserId UserId => Context.User!.GetRequiredUserId();
}
