using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs;
using Shared.Extensions;
using Shared.Services;
using Squadtalk.Data;
using Squadtalk.Repositories;

namespace Squadtalk.Services.Prerender;

public class LocalCommunicationService : ICommunicationService, IPersistState
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly UserRepository _userRepository;
    private readonly PersistentComponentState _persistentComponentState;
    private readonly ChatConnectionManager _connectionManager;

    public LocalCommunicationService(
        AuthenticationStateProvider authenticationStateProvider,
        UserRepository userRepository,
        PersistentComponentState persistentComponentState,
        ChatConnectionManager connectionManager)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _userRepository = userRepository;
        _persistentComponentState = persistentComponentState;
        _connectionManager = connectionManager;
    }

    public event Func<IChatMessage, Task>? MessageReceived;
    public event Func<IChatUser, Task>? UserConnected;
    public event Func<IChatUser, Task>? UserDisconnected;
    public event Func<IEnumerable<IChatUser>, bool, Task>? ConnectedUsersReceived;
    public event Func<string, Task>? ConnectionStatusChanged;
    public event Func<IEnumerable<IChatChannel>, Task>? ChannelsReceived;
    public event Func<IChatChannel, Task>? AddedToChannel;
    public event Func<ChannelId, string?, Task>? ChannelNameChanged;
    public event Func<ChannelId, IChatUser, Task>? CallAccepted;
    public event Func<ChannelId, UserId, Task>? IncomingCall;
    public event Func<IChatUser, ChannelId, Task>? CallDeclined;
    public event Func<ChannelId, Task>? CallEnded;
    public event Func<string, Task>? CallFailed;

    public string ConnectionStatus => ICommunicationService.Online;
    public bool Connected => true;

    private Task? _connectTask;
    private readonly List<ChannelDto> _channelsToPersist = [];

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

        var channelDtos = user.Channels.Select(x => x.ToDto()).ToList();
        foreach (var channelDto in channelDtos)
        {
            _channelsToPersist.Add(channelDto);
        }

        await ConnectedUsersReceived.TryInvoke(_connectionManager.ConnectedUsers, true);
        await ChannelsReceived.TryInvoke(channelDtos);
    }

    public Task PersistDataAsync()
    {
        _persistentComponentState.PersistAsJson(IPersistState.Channels, _channelsToPersist);

        var userDtos = _connectionManager.ConnectedUsers.Select(x => x.ToDto());
        _persistentComponentState.PersistAsJson(IPersistState.Users, userDtos);

        return Task.CompletedTask;
    }

    public Task SendMessageAsync(string content, ChannelId channelId, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task<RoomTokenDto?> StartVoiceCallAsync(ChannelId channelId)
    {
        return Task.FromResult<RoomTokenDto?>(null);
    }

    public Task<RoomTokenDto?> AcceptCallAsync(ChannelId channelId)
    {
        return Task.FromResult<RoomTokenDto?>(null);
    }

    public Task DeclineCallAsync(ChannelId channelId)
    {
        return Task.CompletedTask;
    }

    public Task<bool> ChannelHasActiveCall(ChannelId channelId)
    {
        return Task.FromResult(false);
    }

    public Task<bool> ChangeChannelNameAsync(string? newName, ChannelId channelId)
    {
        return Task.FromResult(false);
    }

    public Task<TimeSpan> MeasureClientDelayAsync()
    {
        return Task.FromResult<TimeSpan>(default);
    }
}
