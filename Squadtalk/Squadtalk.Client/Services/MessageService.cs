using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Shared.Communication;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Extensions;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Client.Services;

public class MessageService : IMessageService
{
    private readonly ILogger<MessageService> _logger;
    private readonly IMessageModelService _modelService;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IMessagePageProvider _messagePageProvider;
    private readonly ICommunicationService _communicationService;
    private readonly ITextChatService _textChatService;
    
    private UserId? _userId;
    
    public event Func<ChannelId, Task>? MessageReceived;
    
    public MessageService(
        ITextChatService textChatService,
        IMessageModelService modelService,
        AuthenticationStateProvider authenticationStateProvider,
        IMessagePageProvider messagePageProvider,
        ICommunicationService communicationService,
        ILogger<MessageService> logger)
    {
        _textChatService = textChatService;
        _modelService = modelService;
        _authenticationStateProvider = authenticationStateProvider;
        _messagePageProvider = messagePageProvider;
        _communicationService = communicationService;
        _logger = logger;

        _communicationService.MessageReceived += HandleIncomingMessage;
    }

    private async Task HandleIncomingMessage(IChatMessage messageDto)
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
            state.Cursor = new TextChannelCursor(DateTimeOffset.UtcNow.UtcTicks);
        }

        await MessageReceived.TryInvoke(messageDto.ChannelId);
    }

    public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        if (_textChatService.CurrentChannel is not { Id: var channelId }) return;

        await _communicationService.SendMessageAsync(message, channelId, cancellationToken);

        _textChatService.CurrentChannel.SetLastMessage(message, DateTimeOffset.Now, true);
    }

    private async Task UpdateChannelMessageState(TextChannelModel textChannelModel, IChatMessage message)
    {
        if (_userId is null)
        {
            var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            _userId = UserId.Parse(authenticationState.User.GetRequiredClaimValue(ClaimTypes.NameIdentifier));
        }

        var messageByCurrentUser = message.Author.Id == _userId;

        if (_textChatService.CurrentChannel != textChannelModel && !messageByCurrentUser)
        {
            textChannelModel.State.UnreadMessages++;
        }

        textChannelModel.SetLastMessage(message, messageByCurrentUser);
    }
    
    public async Task<IList<MessageModel>> GetMessagePageAsync(ChannelId id, CancellationToken cancellationToken)
    {
        var channel = _textChatService.GetChannel(id);
        if (channel is null or { State.ReachedEnd: true })
        {
            return Array.Empty<MessageModel>();
        }

        var channelState = channel.State;
        // var cursor = new TextChannelCursor(channelState.Cursor);
        var page = await _messagePageProvider.GetPageAsync(id, channelState.Cursor, cancellationToken);

        if (page.Count == 0)
        {
            return Array.Empty<MessageModel>();
        }

        channelState.Cursor = new TextChannelCursor(page[0].Timestamp.UtcTicks);
        return _modelService.CreateModelPage(page, channelState);
    }
}