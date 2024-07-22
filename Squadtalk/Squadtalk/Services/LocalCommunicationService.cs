using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs;
using Shared.Extensions;
using Shared.Models;
using Shared.Services;
using Squadtalk.Data;
using Squadtalk.Data.Entities;
using Squadtalk.Hubs;

namespace Squadtalk.Services;

public class LocalCommunicationService : ICommunicationService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly MessageStorageService _messageStorageService;
    private readonly ChatConnectionManager _connectionManager;
    private readonly LocalMessageNotificationService _notificationService;

    public LocalCommunicationService(
        AuthenticationStateProvider authenticationStateProvider,
        ApplicationDbContext dbContext,
        IHubContext<ChatHub, IChatClient> hubContext,
        MessageStorageService messageStorageService,
        ChatConnectionManager connectionManager,
        LocalMessageNotificationService notificationService)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _dbContext = dbContext;
        _hubContext = hubContext;
        _messageStorageService = messageStorageService;
        _connectionManager = connectionManager;
        _notificationService = notificationService;
        
        _notificationService.MessageSent += NotificationServiceOnMessageSent;
    }

    public event Func<IChatMessage, Task>? MessageReceived;
    public event Func<IChatUser, Task>? UserConnected;
    public event Func<IChatUser, Task>? UserDisconnected;
    public event Func<IEnumerable<IChatUser>, Task>? ConnectedUsersReceived;
    public event Func<string, Task>? ConnectionStatusChanged;
    public event Func<IEnumerable<IChatChannel>, Task>? ChannelsReceived;
    public event Func<IChatChannel, Task>? AddedToChannel;
    public event Func<ChannelId, IChatUser, Task>? CallAccepted;
    public event Func<ChannelId, UserId, Task>? IncomingCall;
    public event Func<IChatUser, ChannelId, Task>? CallDeclined;
    public event Func<ChannelId, Task>? CallEnded;
    public event Func<string, Task>? CallFailed;

    public string ConnectionStatus => ICommunicationService.Online;
    public bool Connected => true;
    
    private Task NotificationServiceOnMessageSent(Message message)
    {
        return MessageReceived.TryInvoke(message);
    }

    public Task ConnectAsync()
    {
        return Task.CompletedTask;
    }

    public async Task SendMessageAsync(string content, ChannelId channelId, CancellationToken cancellationToken)
    {
        var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var id = Guid.Parse(authenticationState.User.GetRequiredClaimValue(ClaimTypes.NameIdentifier));
        var userId = new UserId(id);
        
        var user = await _dbContext.Users
            .AsSplitQuery()
            .Include(x => x.Channels)
            .ThenInclude(x => x.Participants)
            .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
        
        ArgumentNullException.ThrowIfNull(user);
        
        if (channelId != GroupChatModel.GlobalChatId && !user.Channels.Exists(x => x.Id == channelId))
        {
            return;
        }
        
        var message = _messageStorageService.CreateMessage(user, content, channelId);
        await _messageStorageService.StoreMessageAsync(message);
        
        var dto = message.ToDto();
        await _hubContext.Clients.Group(channelId).ReceiveMessage(dto);
    }

    Task<RoomTokenDto?> ICommunicationService.StartVoiceCallAsync(ChannelId channelId)
    {
        throw new NotImplementedException();
    }

    public Task<RoomTokenDto?> AcceptCallAsync(ChannelId channelId)
    {
        throw new NotImplementedException();
    }

    public Task DeclineCallAsync(ChannelId channelId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ChannelHasActiveCall(ChannelId channelId)
    {
        throw new NotImplementedException();
    }
}