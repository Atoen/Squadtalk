using System.Runtime.CompilerServices;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Shared.Communication;
using Shared.Data;
using Shared.DTOs;
using Shared.Extensions;
using Squadtalk.Data;
using Squadtalk.Services;

namespace Squadtalk.Hubs;

[Authorize]
public partial class ChatHub : Hub<IChatClient>
{
    private readonly ChatConnectionManager<ApplicationUser, UserId> _connectionManager;
    private readonly ILogger<ChatHub> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly VoiceCallManager _voiceCallManager;

    public ChatHub(ChatConnectionManager<ApplicationUser, UserId> connectionManager,
        ILogger<ChatHub> logger,
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        VoiceCallManager voiceCallManager)
    {
        _connectionManager = connectionManager;
        _logger = logger;
        _dbContext = dbContext;
        _userManager = userManager;
        _voiceCallManager = voiceCallManager;
    }

    private IVoiceChatClient VoiceClient(string connectionId) => Clients.Client(connectionId);
    private IVoiceChatClient VoiceGroup(string groupName) => Clients.Group(groupName);
    private IVoiceChatClient OthersInVoiceGroup(string groupName) => Clients.OthersInGroup(groupName);
    private IVoiceChatClient VoiceCaller => Clients.Caller;
    
    private ITextChatClient TextGroup(string groupName) => Clients.Group(groupName);
    private ITextChatClient TextClient(string connectionId) => Clients.Client(connectionId);
    private ITextChatClient TextCaller => Clients.Caller;
    
    private async Task<ApplicationUser?> GetUserWithChannelsAsync(ClaimsPrincipal? principal,
        [CallerMemberName] string? callerMemberName = null)
    {
        if (principal is not { Identity.IsAuthenticated: true })
        {
            _logger.LogWarning("{Method}: Unable to get user data", callerMemberName);
            return null;
        }
        
        var id = new UserId(principal.GetRequiredClaimValue(ClaimTypes.NameIdentifier));
        
        var user = await _dbContext.Users
            .AsSplitQuery()
            .Include(x => x.Channels)
            .ThenInclude(x => x.Participants)
            .SingleOrDefaultAsync(x => x.Id == id);

        if (user is null)
        {
            _logger.LogWarning("Unable to access user data");
        }
        
        return user;
    }

    private bool UserParticipatesInChannel(ApplicationUser user, ChannelId id)
    {
        return user.Channels.Exists(x => x.Id == id);
    }
    
    private async Task AddUserToPrivateChannelsAsync(UserDto user, List<Channel> channels, bool isUniqueUserConnection)
    {
        foreach (var channel in channels)
        {
            if (isUniqueUserConnection)
            {
                await TextGroup(channel.Id).UserConnected(user);
            }
            
            await Groups.AddToGroupAsync(Context.ConnectionId, channel.Id);
        }
    }

    public async Task SendMessage(string messageContent, ChannelId id, MessageStorageService messageStorageService)
    {
        var user = await GetUserWithChannelsAsync(Context.User);
        if (user is null) return;

        if (id != GroupChat.GlobalChatId && !UserParticipatesInChannel(user, id))
        {
            return;
        }

        var message = messageStorageService.CreateMessage(user, messageContent, id);
        await messageStorageService.StoreMessageAsync(message);

        var dto = message.ToDto();
        await TextGroup(id).ReceiveMessage(dto);
    }

    public override async Task OnConnectedAsync()
    {
        var user = await GetUserWithChannelsAsync(Context.User);
        if (user is null) return;

        var dto = user.ToDto();

        var isUniqueConnection = await _connectionManager.Add(user, Context.ConnectionId);
        if (isUniqueConnection)
        {
            await TextGroup(GroupChat.GlobalChatId).UserConnected(dto);
        }
        
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupChat.GlobalChatId);
        
        var channelDtos = user.Channels.Select(x => x.ToDto()).ToList();
        
        await TextCaller.GetChannels(channelDtos);
        await TextCaller.GetConnectedUsers(_connectionManager.ConnectedUsers.Select(x => x.ToDto()).ToList());

        if (user.Channels is not { Count: > 0 }) return;

        await AddUserToPrivateChannelsAsync(dto, user.Channels, isUniqueConnection);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = await GetUserWithChannelsAsync(Context.User);
        if (user is null) return;
        
        await EndCall();

        var dto = user.ToDto();
        
        var allConnectionsClosed = await _connectionManager.Remove(user, Context.ConnectionId);
        if (!allConnectionsClosed) return;

        await TextGroup(GroupChat.GlobalChatId).UserDisconnected(dto);
        if (user.Channels is not { Count: > 0 }) return;

        foreach (var channel in user.Channels)
        {
            await TextGroup(channel.Id).UserDisconnected(dto);
        }
    }
}