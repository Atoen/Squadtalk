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
    private readonly ILogger<ConnectionService> _logger;

    event Func<string, Task>? IConnectionService.ConnectionStatusChanged { add { } remove { } }

    public string ConnectionStatus => "Connecting";

    public bool Connected => false;

    private Task? _connectTask;

    public ConnectionService(
        AuthenticationStateProvider authenticationStateProvider,
        UserRepository userRepository,
        PrerenderPersistantState persistState,
        ChatConnectionManager chatConnectionManager,
        ILogger<ConnectionService> logger)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _userRepository = userRepository;
        _persistState = persistState;
        _chatConnectionManager = chatConnectionManager;
        _logger = logger;

        _logger.LogInformation("Created connection service");
    }

    public Task<TimeSpan> MeasureConnectionDelayAsync() => Task.FromResult<TimeSpan>(default);

    public Task OnPersisting()
    {
        _persistState.PersistData();

        return Task.CompletedTask;
    }

    public async Task ConnectAsync()
    {
        _logger.LogInformation("Called");

        _connectTask ??= ConnectInternalAsync();
        await _connectTask;
    }

    private async Task ConnectInternalAsync()
    {
        _logger.LogInformation("Storing data");

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

        // _persistState.Channels = channelDtos;
        // _persistState.Users = users;

        _persistState.AddData(channelDtos, users);

        _logger.LogInformation("Finished storing data");
    }
}
