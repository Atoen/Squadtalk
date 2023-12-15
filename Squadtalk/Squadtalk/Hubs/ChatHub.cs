using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Shared.Communication;
using Shared.DTOs;
using Squadtalk.Data;
using Squadtalk.Services;

namespace Squadtalk.Hubs;

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private readonly ChatConnectionManager<UserDto> _connectionManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ChatConnectionManager<UserDto> connectionManager, UserManager<ApplicationUser> userManager, 
        ILogger<ChatHub> logger)
    {
        _connectionManager = connectionManager;
        _userManager = userManager;
        _logger = logger;
    }

    private async Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal? principal)
    {
        if (principal is not {Identity.IsAuthenticated: true })
        {
            return null;
        }
        
        return await _userManager.GetUserAsync(principal);
    }

    private bool CheckIfUserParticipatesInChannel(ApplicationUser user, string channelId)
    {
        return user.Channels.Exists(x => x.Id == channelId);
    }
    
    private async Task AddUserToPrivateChannelsAsync(UserDto user, List<Channel> channels, bool isUniqueUserConnection)
    {
        foreach (var channel in channels)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, channel.Id);
            if (isUniqueUserConnection)
            {
                await Clients.Group(channel.Id).UserConnected(user);
            }
        }
    }

    public async Task SendMessage(string messageContent, string channelId, MessageStorageService messageStorageService)
    {
        var user = await GetUserAsync(Context.User);
        
        if (user is null)
        {
            _logger.LogWarning("SendMessage: Unable to get user data");
            return;
        }

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
        if (user is null)
        {
            _logger.LogWarning("OnConnectedAsync: Unable to get user data");
            return;
        }

        var dto = user.ToDto();

        var isUniqueConnection = await _connectionManager.Add(dto, Context.ConnectionId);

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupChat.GlobalChatId);
        if (isUniqueConnection)
        {
            await Clients.Group(GroupChat.GlobalChatId).UserConnected(dto);
        }
        
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
        if (user is null)
        {
            _logger.LogWarning("OnDisconnectedAsync: Unable to get user data");
            return;
        }
        
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