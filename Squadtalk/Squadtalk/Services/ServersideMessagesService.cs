using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Communication;
using Shared.Data;
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
    private readonly ITextChatService _textChatService;
    private readonly IMessageModelService<Message> _modelService;

    public event Func<ChannelId, Task>? MessageReceived;

    public ServersideMessagesService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, 
        AuthenticationStateProvider authenticationStateProvider, ILogger<ServersideMessagesService> logger,
        ITextChatService textChatService, IMessageModelService<Message> modelService)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _authenticationStateProvider = authenticationStateProvider;
        _logger = logger;
        _textChatService = textChatService;
        _modelService = modelService;
    }
    
    public async Task<IList<MessageModel>> GetMessagePageAsync(ChannelId id, CancellationToken cancellationToken)
    {
        var channel = _textChatService.GetChannel(id);
        
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

        if (id != GroupChat.GlobalChatId && !user.Channels.Exists(x => x.Id == id))
        {
            return ArraySegment<MessageModel>.Empty;
        }

        var state = channel.State;

        var cursor = state.Cursor == default
            ? default
            : new DateTimeOffset(state.Cursor, TimeSpan.Zero);
        
        var messages = await GetMessages(cursor, id, cancellationToken);
        
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

    private async Task<List<Message>> GetMessages(DateTimeOffset cursor, ChannelId id, CancellationToken 
            cancellationToken)
    {
        return cursor == default
            ? await _dbContext.Messages
                .AsNoTracking()
                .OrderByDescending(m => m.Timestamp)
                .Where(m => m.ChannelId == id)
                .Take(20)
                .Include(m => m.Author)
                .Reverse()
                .ToListAsync(cancellationToken)
            
            : await _dbContext.Messages
                .AsNoTracking()
                .OrderByDescending(m => m.Timestamp)
                .Where(m => m.ChannelId == id)
                .Where(m => m.Timestamp < cursor)
                .Take(20)
                .Include(m => m.Author)
                .Reverse()
                .ToListAsync(cancellationToken);
    }
}