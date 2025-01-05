using Microsoft.AspNetCore.Components.Authorization;
using Shared.Services;
using Squadtalk.Data;
using Squadtalk.Repositories;

namespace Squadtalk.Services.Prerender;

internal class ConnectionService : IConnectionService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly UserRepository _userRepository;
    private readonly FriendRepository _friendRepository;
    private readonly PrerenderPersistantState _persistState;

    event Func<ConnectionStatus, Task>? IConnectionService.ConnectionStatusChanged { add { } remove { } }

    public ConnectionStatus ConnectionStatus => ConnectionStatus.Connecting;

    private Task? _connectTask;

    public ConnectionService(
        AuthenticationStateProvider authenticationStateProvider,
        UserRepository userRepository,
        FriendRepository friendRepository,
        PrerenderPersistantState persistState)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _userRepository = userRepository;
        _friendRepository = friendRepository;
        _persistState = persistState;
    }

    public Task<TimeSpan> MeasureConnectionDelayAsync() => Task.FromResult<TimeSpan>(default);

    public Task OnPersisting()
    {
        _persistState.PersistData();

        return Task.CompletedTask;
    }

    public async Task ConnectAsync()
    {
        _connectTask ??= ConnectInternalAsync();
        await _connectTask;
    }

    private async Task ConnectInternalAsync()
    {
        var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = await _userRepository.FindUserByid(authenticationState.User, ChannelsInclusionOption.IncludeWithParticipants);
        if (user is null)
        {
            return;
        }

        var friends = await _friendRepository.GetUserFriendsAsync(user.Id);
        var friendRequests = await _friendRepository.GetUserPendingFriendRequests(user.Id);

        var channelDtos = user.Channels
            .Select(x => x.ToDto())
            .OrderByDescending(x => x.LastMessage?.Timestamp)
            .ToList();

        var friendDtos = friends
            .Select(x => x.ToDto())
            .ToList();

        var friendRequestDtos = friendRequests
            .Select(x => x.ToDto())
            .ToList();

        _persistState.AddData(channelDtos, friendDtos, friendRequestDtos);
    }
}
