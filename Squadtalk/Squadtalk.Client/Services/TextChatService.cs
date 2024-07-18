using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Shared.Communication;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Extensions;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Client.Services;

public class TextChatService : ITextChatService
{
    private readonly ILogger<TextChatService> _logger;
    private readonly IMessageModelService _modelService;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IMessagePageProvider _messagePageProvider;
    private readonly ICommunicationService _communicationService;
    private readonly IChatService _chatService;
    
    private UserId? _userId;
    private MessageModel? _callStartedMessageModel;
    
    public event Func<ChannelId, Task>? MessageReceived;
    
    public TextChatService(
        IChatService chatService,
        IMessageModelService modelService,
        AuthenticationStateProvider authenticationStateProvider,
        IMessagePageProvider messagePageProvider,
        ICommunicationService communicationService,
        ILogger<TextChatService> logger)
    {
        _chatService = chatService;
        _modelService = modelService;
        _authenticationStateProvider = authenticationStateProvider;
        _messagePageProvider = messagePageProvider;
        _communicationService = communicationService;
        _logger = logger;

        _communicationService.MessageReceived += HandleIncomingMessage;
    }

    public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        if (_chatService.CurrentChannel is not { Id: var channelId }) return;

        await _communicationService.SendMessageAsync(message, channelId, cancellationToken);

        _chatService.CurrentChannel.SetLastMessage(message, DateTimeOffset.Now, ChannelModel.CurrentUserAuthorPrefix);
    }

    public async Task<IList<MessageModel>> GetMessagePageAsync(ChannelId id, CancellationToken cancellationToken)
    {
        var channel = _chatService.GetChannel(id);
        if (channel is null or { State.ReachedEnd: true })
        {
            return Array.Empty<MessageModel>();
        }

        var channelState = channel.State;
        var page = await _messagePageProvider.GetPageAsync(id, channelState.Cursor, cancellationToken);

        if (page.Count == 0)
        {
            return Array.Empty<MessageModel>();
        }

        channelState.Cursor = new TextChannelCursor(page[0].Timestamp.UtcTicks);
        return _modelService.CreateModelPage(page, channelState);
    }

    private async Task HandleIncomingMessage(IChatMessage messageDto)
    {
        var channel = _chatService.GetChannel(messageDto.ChannelId);
        if (channel is null)
        {
            _logger.LogWarning("Received message on nonexistent channel id: {Id}", messageDto.ChannelId);
            return;
        }

        await UpdateChannelMessageState(channel, messageDto);
        
        var channelState = channel.State;
        var message = _modelService.CreateModel(messageDto, channelState, false);

        channelState.AddMessage(message);

        await MessageReceived.TryInvoke(messageDto.ChannelId);
    }

    private async Task UpdateChannelMessageState(ChannelModel channelModel, IChatMessage message)
    {
        if (_userId is null)
        {
            var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            _userId = UserId.Parse(authenticationState.User.GetRequiredClaimValue(ClaimTypes.NameIdentifier));
        }

        var messageByCurrentUser = message.Author.Id == _userId;

        if (_chatService.CurrentChannel != channelModel && !messageByCurrentUser)
        {
            channelModel.State.UnreadMessages++;
        }

        channelModel.SetLastMessage(message, messageByCurrentUser);
    }
}
