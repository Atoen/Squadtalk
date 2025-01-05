using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Shared.Data.TypedIds;
using Shared.Models;
using Shared.Signalr.Clients;
using Squadtalk.Data.Entities;
using Squadtalk.Extensions;
using Squadtalk.Repositories;
using Squadtalk.Services;

namespace Squadtalk.Signalr.Hubs;

[Authorize]
public partial class AppHub : Hub<IChatClient>
{
    private readonly HubConnectionManager _connectionManager;
    private readonly UserRepository _userRepository;

    public AppHub(
        HubConnectionManager connectionManager,
        UserRepository userRepository)
    {
        _connectionManager = connectionManager;
        _userRepository = userRepository;
    }

    public override async Task OnConnectedAsync()
    {
        var user = await _userRepository.FindUserByid(Context.User, ChannelsInclusionOption.Include);
        if (user is null)
        {
            Context.Abort();
            return;
        }

        await _connectionManager.AddAsync(user, Context.ConnectionId);

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupChatModel.GlobalChatId);

        if (user.Channels is { Count: > 0 })
        {
            var tasks = user.Channels
                .Select(x => Groups.AddToGroupAsync(Context.ConnectionId, x.Id));

            await Task.WhenAll(tasks);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = await _userRepository.FindUserByid(Context.User);
        if (user is null)
        {
            return;
        }

        await _connectionManager.RemoveAsync(user, Context.ConnectionId);
    }

    private async Task<ApplicationUser?> GetChannelParticipantAsync(ChannelId channelId)
    {
        var user = await _userRepository.FindUserByid(Context.User, ChannelsInclusionOption.Include);
        if (user is null || !user.ParticipatesInChannel(channelId))
        {
            return null;
        }

        return user;
    }
}
