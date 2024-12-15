using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Extensions;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Client.Services;

internal class TextChatService : ITextChatService
{
    private readonly ILogger<TextChatService> _logger;
    private readonly IMessageModelService _modelService;
    private readonly IMessagePageProvider _messagePageProvider;
    private readonly ICommunicationService _communicationService;
    private readonly IUserAuthenticationService _userAuthenticationService;
    private readonly IChatService _chatService;

    public event Func<ChannelId, Task>? MessageReceived;

    public TextChatService(
        IChatService chatService,
        IMessageModelService modelService,
        IMessagePageProvider messagePageProvider,
        ICommunicationService communicationService,
        IUserAuthenticationService userAuthenticationService,
        ILogger<TextChatService> logger)
    {
        _chatService = chatService;
        _modelService = modelService;
        _messagePageProvider = messagePageProvider;
        _communicationService = communicationService;
        _userAuthenticationService = userAuthenticationService;
        _logger = logger;

        _communicationService.MessageReceived += HandleIncomingMessage;
    }

    public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        if (_chatService.CurrentChannel is not { Id: var channelId }) return;

        await _communicationService.SendMessageAsync(message, channelId, cancellationToken);
    }

    public async Task<IList<MessageModel>> GetMessagePageAsync(ChannelId id, CancellationToken cancellationToken)
    {
        var channel = _chatService.GetChannel(id);
        if (channel is null or { State.ReachedEnd: true })
        {
            _logger.LogInformation("channel null");

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

        UpdateChannelMessageState(channel, messageDto);

        var channelState = channel.State;
        var message = _modelService.CreateModel(messageDto, channelState, false);

        channelState.AddMessage(message);

        await MessageReceived.TryInvoke(messageDto.ChannelId);
    }

    private void UpdateChannelMessageState(ChannelModel channelModel, IChatMessage message)
    {
        var messageByCurrentUser = message.Author.Id == _userAuthenticationService.UserId;

        if (_chatService.CurrentChannel != channelModel && !messageByCurrentUser)
        {
            channelModel.State.UnreadMessages++;
        }

        channelModel.LastMessage = message;
    }
}
