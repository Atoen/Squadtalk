using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Communication;
using Shared.Data;
using Shared.DTOs;
using Shared.Extensions;
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
    private readonly IMessageModelService<MessageDto> _modelService;
    private readonly ISignalrService _signalrService;
    
    private UserId? _userId;

    public event Func<ChannelId, Task>? MessageReceived;

    public ServersideMessagesService(ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager, 
        AuthenticationStateProvider authenticationStateProvider,
        ILogger<ServersideMessagesService> logger,
        ITextChatService textChatService,
        IMessageModelService<MessageDto> modelService,
        ISignalrService signalrService)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _authenticationStateProvider = authenticationStateProvider;
        _logger = logger;
        _textChatService = textChatService;
        _modelService = modelService;
        _signalrService = signalrService;
        
        _signalrService.MessageReceived += HandleIncomingMessage;
    }

    private async Task HandleIncomingMessage(MessageDto messageDto)
    {
        var channel = _textChatService.GetChannel(messageDto.ChannelId);
        if (channel is null)
        {
            _logger.LogWarning("Received message on nonexistent channel id: {Id}", messageDto.ChannelId);
            return;
        }
        
        await UpdateChannelMessageState(channel, messageDto);

        var state = channel.State;
        var message = _modelService.CreateModel(messageDto, state, false);

        state.Messages.Add(message);
        state.LastMessageReceived = message;

        if (state.Cursor == default)
        {
            state.Cursor = DateTimeOffset.UtcNow.UtcTicks;
        }

        await MessageReceived.TryInvoke(messageDto.ChannelId);
    }
    
    private async Task UpdateChannelMessageState(TextChannel textChannel, MessageDto messageDto)
    {
        if (_userId is null)
        {
            var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            _userId = new UserId(authenticationState.User.GetRequiredClaimValue(ClaimTypes.NameIdentifier));
        }

        var messageByCurrentUser = messageDto.Author.Id == _userId;

        if (_textChatService.CurrentChannel != textChannel && !messageByCurrentUser)
        {
            textChannel.State.UnreadMessages++;
        }

        textChannel.SetLastMessage(messageDto, messageByCurrentUser);
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
        var dtos = messages.Select(x => x.ToDto()).ToList();
        
        if (dtos.Count > 0)
        {
            state.Cursor = dtos[0].Timestamp.UtcTicks;
        }
        
        return _modelService.CreateModelPage(dtos, state);
    }

    public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        if (_textChatService.CurrentChannel is not { Id: { } id }) return;

        await _signalrService.SendMessageAsync(message, id, cancellationToken);
        
        _textChatService.CurrentChannel.SetLastMessage(message, DateTimeOffset.Now, true);
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