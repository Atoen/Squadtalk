using System.Runtime.CompilerServices;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Shared.Communication;
using Shared.DTOs;
using Shared.Extensions;
using Squadtalk.Data;
using Squadtalk.Services;

namespace Squadtalk.Hubs;

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private readonly ChatConnectionManager<UserDto, string> _connectionManager;
    private readonly ILogger<ChatHub> _logger;
    private readonly ApplicationDbContext _dbContext;

    public ChatHub(ChatConnectionManager<UserDto, string> connectionManager, ILogger<ChatHub> logger, ApplicationDbContext dbContext)
    {
        _connectionManager = connectionManager;
        _logger = logger;
        _dbContext = dbContext;
    }

    private async Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal? principal, [CallerMemberName] string? callerMemberName = null)
    {
        if (principal is not {Identity.IsAuthenticated: true })
        {
            _logger.LogWarning("{Method}: Unable to get user data", callerMemberName);
            return null;
        }

        var id = principal.GetRequiredClaimValue(ClaimTypes.NameIdentifier);

        var user = await _dbContext.Users
            .Include(x => x.Channels)
            .ThenInclude(x => x.Participants)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user is null)
        {
            _logger.LogWarning("Unable to access user data with id: {Id}", id);
        }
        
        return user;
    }

    private bool CheckIfUserParticipatesInChannel(ApplicationUser user, string channelId)
    {
        return user.Channels.Exists(x => x.Id == channelId);
    }
    
    private async Task AddUserToPrivateChannelsAsync(UserDto user, List<Channel> channels, bool isUniqueUserConnection)
    {
        foreach (var channel in channels)
        {
            if (isUniqueUserConnection)
            {
                await Clients.Group(channel.Id).UserConnected(user);
            }
            
            await Groups.AddToGroupAsync(Context.ConnectionId, channel.Id);
        }
    }

    public async Task SendMessage(string messageContent, string channelId, MessageStorageService messageStorageService)
    {
        var user = await GetUserAsync(Context.User);
        if (user is null) return;

        if (channelId != GroupChat.GlobalChatId && !CheckIfUserParticipatesInChannel(user, channelId))
        {
            return;
        }

        var message = messageStorageService.CreateMessage(user, messageContent, channelId);
        await messageStorageService.StoreMessageAsync(message);

        var dto = message.ToDto();
        await Clients.Group(channelId).ReceiveMessage(dto);
    }

    public override async Task OnConnectedAsync()
    {
        var user = await GetUserAsync(Context.User);
        if (user is null) return;

        var dto = user.ToDto();

        var isUniqueConnection = await _connectionManager.Add(dto, Context.ConnectionId);

        if (isUniqueConnection)
        {
            await Clients.Group(GroupChat.GlobalChatId).UserConnected(dto);
        }
        
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupChat.GlobalChatId);
        
        var channelDtos = user.Channels.Select(x => x.ToDto());
        await Clients.Caller.GetChannels(channelDtos);
        await Clients.Caller.GetConnectedUsers(_connectionManager.ConnectedUsers);

        if (user.Channels is not { Count: > 0 })
        {
            return;
        }

        await AddUserToPrivateChannelsAsync(dto, user.Channels, isUniqueConnection);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = await GetUserAsync(Context.User);
        if (user is null) return;
        
        var dto = user.ToDto();
        
        var allConnectionsClosed = await _connectionManager.Remove(dto, Context.ConnectionId);
        if (!allConnectionsClosed) return;

        await Clients.Group(GroupChat.GlobalChatId).UserDisconnected(dto);
        if (user.Channels is not { Count: > 0 })
        {
            return;
        }

        foreach (var channel in user.Channels)
        {
            await Clients.Group(channel.Id).UserDisconnected(dto);
        }
    }
}