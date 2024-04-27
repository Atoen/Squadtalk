using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Shared.Communication;
using Shared.Data;
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
    
    private string? _userId;
    
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
            state.Cursor = DateTimeOffset.UtcNow.UtcTicks;
        }

        await MessageReceived.TryInvoke(messageDto.ChannelId);
    }

    public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        if (_textChatService.CurrentChannel is not { Id: { } id }) return;

        await _communicationService.SendMessageAsync(message, id, cancellationToken);

        _textChatService.CurrentChannel.SetLastMessage(message, DateTimeOffset.Now, true);
    }

    private async Task UpdateChannelMessageState(TextChannelModel textChannelModel, IChatMessage message)
    {
        if (_userId is null)
        {
            var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            _userId = authenticationState.User.GetRequiredClaimValue(ClaimTypes.NameIdentifier);
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
        var cursor = new MessageCursor(channelState.Cursor);
        var page = await _messagePageProvider.GetPageAsync(id, cursor, cancellationToken);

        if (page.Count == 0)
        {
            return Array.Empty<MessageModel>();
        }

        channelState.Cursor = page[0].Timestamp.UtcTicks;
        return _modelService.CreateModelPage(page, channelState);
    }
}