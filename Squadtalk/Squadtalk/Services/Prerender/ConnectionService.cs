using Microsoft.AspNetCore.Components.Authorization;
using Shared.Services;
using Squadtalk.Data;
using Squadtalk.Repositories;

namespace Squadtalk.Services.Prerender;

internal class ConnectionService : IConnectionService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly UserRepository _userRepository;
    private readonly PrerenderPersistantState _persistState;
    private readonly ChatConnectionManager _chatConnectionManager;

    event Func<ConnectionStatus, Task>? IConnectionService.ConnectionStatusChanged { add { } remove { } }

    public ConnectionStatus ConnectionStatus => ConnectionStatus.Connecting;

    private Task? _connectTask;

    public ConnectionService(
        AuthenticationStateProvider authenticationStateProvider,
        UserRepository userRepository,
        PrerenderPersistantState persistState,
        ChatConnectionManager chatConnectionManager)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _userRepository = userRepository;
        _persistState = persistState;
        _chatConnectionManager = chatConnectionManager;
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
        var user = await _userRepository.GetUserAsync(authenticationState.User, ChannelsInclusionOption.IncludeWithParticipants);
        if (user is null)
        {
            return;
        }

        var users = _chatConnectionManager.ConnectedUsers.Select(x => x.ToDto()).ToList();
        var channelDtos = user.Channels
            .Select(x => x.ToDto())
            .OrderByDescending(x => x.LastMessage?.Timestamp)
            .ToList();

        _persistState.AddData(channelDtos, users);
    }
}
