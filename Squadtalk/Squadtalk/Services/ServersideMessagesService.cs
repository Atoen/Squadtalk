using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Communication;
using Shared.Models;
using Shared.Services;
using Squadtalk.Data;

namespace Squadtalk.Services;

public class ServersideMessagesService : IMessageService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ILogger<ServersideMessagesService> _logger;
    private readonly ICommunicationManager _communicationManager;
    private readonly IMessageModelService<Message> _modelService;

    public event Func<string, Task>? MessageReceived;

    public ServersideMessagesService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, 
        AuthenticationStateProvider authenticationStateProvider, ILogger<ServersideMessagesService> logger,
        ICommunicationManager communicationManager, IMessageModelService<Message> modelService)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _authenticationStateProvider = authenticationStateProvider;
        _logger = logger;
        _communicationManager = communicationManager;
        _modelService = modelService;
    }
    
    public async Task<IList<MessageModel>> GetMessagePageAsync(string channelId, CancellationToken cancellationToken)
    {
        var channel = _communicationManager.GetChannel(channelId);
        
        if (channel is null or { State.ReachedEnd: true })
        {
            return ArraySegment<MessageModel>.Empty;
        }
        
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = await _userManager.GetUserAsync(authState.User);
        
        if (user is null)
        {
            _logger.LogWarning("Cannot retrieve user data");
            return ArraySegment<MessageModel>.Empty;
        }

        if (channelId != GroupChat.GlobalChatId && !user.Channels.Exists(x => x.Id == channelId))
        {
            return ArraySegment<MessageModel>.Empty;
        }

        var state = channel.State;

        var cursor = state.Cursor == default
            ? default
            : new DateTimeOffset(state.Cursor, TimeSpan.Zero);
        
        var messages = await GetMessages(cursor, channelId, cancellationToken);
        
        if (messages.Count > 0)
        {
            state.Cursor = messages[0].Timestamp.UtcTicks;
        }
        
        return _modelService.CreateModelPage(messages, state);
    }

    public Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException();
    }

    private async Task<List<Message>> GetMessages(DateTimeOffset cursor, string channelId, CancellationToken cancellationToken)
    {
        return cursor == default
            ? await _dbContext.Messages
                .AsNoTracking()
                .OrderByDescending(m => m.Timestamp)
                .Where(m => m.ChannelId == channelId)
                .Take(20)
                .Include(m => m.Author)
                .Reverse()
                .ToListAsync(cancellationToken)
            
            : await _dbContext.Messages
                .AsNoTracking()
                .OrderByDescending(m => m.Timestamp)
                .Where(m => m.ChannelId == channelId)
                .Where(m => m.Timestamp < cursor)
                .Take(20)
                .Include(m => m.Author)
                .Reverse()
                .ToListAsync(cancellationToken);
    }
}