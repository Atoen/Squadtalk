using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Shared.Communication;
using Shared.Data;
using Shared.DTOs;
using Shared.Extensions;
using Shared.Services;
using Squadtalk.Data;
using Squadtalk.Hubs;

namespace Squadtalk.Services;

public sealed class ServersideSignalrService : ISignalrService
{
    private readonly ChatConnectionManager<ApplicationUser, UserId> _connectionManager;
    private readonly ILogger<ServersideSignalrService> _logger;
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly ApplicationDbContext _dbContext;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IMyCircuit _myCircuit;
    private readonly MessageStorageService _messageStorageService;

#pragma warning disable CS0067

    public event Func<MessageDto, Task>? MessageReceived;
    public event Func<UserDto, Task>? UserConnected;
    public event Func<UserDto, Task>? UserDisconnected;
    public event Func<IEnumerable<UserDto>, Task>? ConnectedUsersReceived;
    public event Func<string, Task>? ConnectionStatusChanged;
    public event Func<IEnumerable<ChannelDto>, Task>? TextChannelsReceived;
    public event Func<ChannelDto, Task>? AddedToTextChannel;
    public event Func<UserDto, CallOfferId, Task>? IncomingCall;
    public event Func<CallOfferId, Task>? CallAccepted;
    public event Func<CallOfferId, Task>? CallDeclined;
    public event Func<CallId, Task>? CallEnded;
    public event Func<string, Task>? CallFailed;
    public event Func<List<UserDto>, CallId, Task>? GetCallUsers;
    public event Func<VoicePacketDto, Task>? GetVoicePacket;

    public ServersideSignalrService(
        ChatConnectionManager<ApplicationUser, UserId> connectionManager,
        ILogger<ServersideSignalrService> logger,
        IHubContext<ChatHub, IChatClient> hubContext,
        ApplicationDbContext dbContext,
        AuthenticationStateProvider authenticationStateProvider,
        IMyCircuit myCircuit,
        NavigationManager navigationManager,
        MessageStorageService messageStorageService)
    {
        _connectionManager = connectionManager;
        _logger = logger;
        _hubContext = hubContext;
        _dbContext = dbContext;
        _authenticationStateProvider = authenticationStateProvider;

        if (myCircuit.CurrentCircuit is not { Id.Length: > 0 })
        {
            navigationManager.NavigateTo("");
        }
        
        _myCircuit = myCircuit;
        _messageStorageService = messageStorageService;
    }

    public Task<CallOfferId?> StartVoiceCallAsync(UserId id)
    {
        throw new InvalidOperationException();
    }

    public Task EndCallAsync(CallId id)
    {
        throw new InvalidOperationException();
    }

    public Task AcceptCallAsync(CallOfferId id)
    {
        throw new InvalidOperationException();
    }

    public Task DeclineCallAsync(CallOfferId id)
    {
        throw new InvalidOperationException();
    }

    public Task StreamDataAsync(CallId callId, IAsyncEnumerable<byte[]> stream, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException();
    }

#pragma warning restore CS0067
    
    public string ConnectionStatus { get; private set; } = ISignalrService.Offline;

    public bool Connected => true;

    public async Task ConnectAsync()
    {
        ConnectionStatus = ISignalrService.Online;
        await ConnectionStatusChanged.TryInvoke(ConnectionStatus);
        
        var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var id = new UserId(authenticationState.User.GetRequiredClaimValue(ClaimTypes.NameIdentifier));
        
        var user = await _dbContext.Users
            .AsSplitQuery()
            .Include(x => x.Channels)
            .ThenInclude(x => x.Participants)
            .SingleOrDefaultAsync(x => x.Id == id);

        ArgumentNullException.ThrowIfNull(user);
        var dto = user.ToDto();

        var isUniqueConnection = await _connectionManager.Add(user, _myCircuit.CurrentCircuit.Id);
        if (isUniqueConnection)
        {
            await _hubContext.Clients.Groups(GroupChat.GlobalChatId).UserConnected(dto);
        }
        
        if (user.Channels is not { Count: > 0 }) return;
        var channelDtos = user.Channels.Select(x => x.ToDto());

        await TextChannelsReceived.TryInvoke(channelDtos);
        await ConnectedUsersReceived.TryInvoke(_connectionManager.ConnectedUsers.Select(x => x.ToDto()));
        
        foreach (var channel in user.Channels.Where(_ => isUniqueConnection))
        {
            await _hubContext.Clients.Groups(channel.Id).UserConnected(dto);
        }
    }

    public async Task SendMessageAsync(string messageContent, ChannelId channelId, CancellationToken cancellationToken = default)
    {
        var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var id = new UserId(authenticationState.User.GetRequiredClaimValue(ClaimTypes.NameIdentifier));
        
        var user = await _dbContext.Users
            .AsSplitQuery()
            .Include(x => x.Channels)
            .ThenInclude(x => x.Participants)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        
        ArgumentNullException.ThrowIfNull(user);

        if (channelId != GroupChat.GlobalChatId && !user.Channels.Exists(x => x.Id == channelId))
        {
            return;
        }
        
        var message = _messageStorageService.CreateMessage(user, messageContent, channelId);
        await _messageStorageService.StoreMessageAsync(message);
        
        var dto = message.ToDto();
        await _hubContext.Clients.Group(channelId).ReceiveMessage(dto);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}