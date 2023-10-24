using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Squadtalk.Server.Models;
using Squadtalk.Server.Services;
using Squadtalk.Shared;

namespace Squadtalk.Server.Hubs;

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private readonly UserService _userService;
    private readonly MessageService _messageService;
    private readonly UserManager _userManager;

    public ChatHub(UserService userService, MessageService messageService, UserManager userManager)
    {
        _userService = userService;
        _messageService = messageService;
        _userManager = userManager;
    }
    
    public async Task SendMessage(string messageContent, IGifSourceVerifier gifSourceVerifier)
    {
        var user = await _userService.GetUserAsync(Context.User!);

        var isGifSource = await gifSourceVerifier.VerifyAsync(messageContent);
        
        var message = new Message
        {
            Author = user.AsT0,
            Timestamp = DateTimeOffset.Now,
            Content = isGifSource ? string.Empty : messageContent
        };

        if (isGifSource)
        {
            message.Embed = new Embed
            {
                Type = EmbedType.Gif,
                Data = new Dictionary<string, string>
                {
                    { "Uri", messageContent }
                }
            };
        }
        
        await _messageService.StoreMessageAsync(message);
        await Clients.All.ReceiveMessage(message);
    }
    
    public override async Task OnConnectedAsync()
    {
        var user = Context.User?.Identity?.Name!;

        var isUniqueUserConnection = await _userManager.UserConnected(user);
        if (isUniqueUserConnection)
        {
            await Clients.Others.UserConnected(user);
        }

        await Clients.Caller.GetConnectedUsers(_userManager.ConnectedUsers);
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = Context.User?.Identity?.Name!;

        var allConnectionsClosed = await _userManager.UserDisconnected(user);
        if (allConnectionsClosed)
        {
            await Clients.Others.UserDisconnected(user);
        }
    }
}