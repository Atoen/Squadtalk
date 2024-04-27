using System.Security.Claims;
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

public class LocalCommunicationService : ICommunicationService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly MessageStorageService _messageStorageService;

    public LocalCommunicationService(
        AuthenticationStateProvider authenticationStateProvider,
        ApplicationDbContext dbContext,
        IHubContext<ChatHub, IChatClient> hubContext,
        MessageStorageService messageStorageService)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _dbContext = dbContext;
        _hubContext = hubContext;
        _messageStorageService = messageStorageService;
    }

    public event Func<VoicePacketDto, Task>? GetVoicePacket;
    public Task ConnectAsync() => Task.CompletedTask;

    public async Task SendMessageAsync(string content, ChannelId channelId, CancellationToken cancellationToken)
    {
        var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var id = new UserId(authenticationState.User.GetRequiredClaimValue(ClaimTypes.NameIdentifier));
        
        var user = await _dbContext.Users
            .AsSplitQuery()
            .Include(x => x.Channels)
            .ThenInclude(x => x.Participants)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        
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

    public Task<CallOfferId?> StartVoiceCallAsync(UserId id)
    {
        throw new NotImplementedException();
    }

    public Task EndCallAsync(CallId id)
    {
        throw new NotImplementedException();
    }

    public Task AcceptCallAsync(CallOfferId id)
    {
        throw new NotImplementedException();
    }

    public Task DeclineCallAsync(CallOfferId id)
    {
        throw new NotImplementedException();
    }

    public Task StreamDataAsync(CallId callId, IAsyncEnumerable<byte[]> stream, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public event Func<IChatMessage, Task>? MessageReceived;
    public event Func<IChatUser, Task>? UserConnected;
    public event Func<IChatUser, Task>? UserDisconnected;
    public event Func<IEnumerable<IChatUser>, Task>? ConnectedUsersReceived;
    public event Func<string, Task>? ConnectionStatusChanged;
    public event Func<IEnumerable<IChatChannel>, Task>? TextChannelsReceived;
    public event Func<IChatChannel, Task>? AddedToTextChannel;
    public event Func<IChatUser, CallOfferId, Task>? IncomingCall;
    public event Func<CallOfferId, Task>? CallAccepted;
    public event Func<CallOfferId, Task>? CallDeclined;
    public event Func<CallId, Task>? CallEnded;
    public event Func<string, Task>? CallFailed;
    public event Func<IEnumerable<IChatUser>, CallId, Task>? GetCallUsers;
}