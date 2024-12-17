using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Extensions;
using Shared.Models;
using Shared.Services;
using Squadtalk.Client.SignalR;

namespace Squadtalk.Client.Services;

internal class TextChatService : ITextChatService
{
    private readonly ILogger<TextChatService> _logger;
    private readonly IMessageModelService _modelService;
    private readonly IMessagePageProvider _messagePageProvider;
    private readonly ISignalrTextService _signalrTextService;
    private readonly IUserAuthenticationService _userAuthenticationService;
    private readonly IChannelManager _channelManager;

    public event Func<ChannelId, MessageModel, Task>? MessageReceived;

    public TextChatService(
        IChannelManager channelManager,
        IMessageModelService modelService,
        IMessagePageProvider messagePageProvider,
        SignalrService signalrTextService,
        IUserAuthenticationService userAuthenticationService,
        ILogger<TextChatService> logger)
    {
        _channelManager = channelManager;
        _modelService = modelService;
        _messagePageProvider = messagePageProvider;
        _signalrTextService = signalrTextService;
        _userAuthenticationService = userAuthenticationService;
        _logger = logger;

        _signalrTextService.MessageReceived += HandleIncomingMessage;
    }

    public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        if (_channelManager.CurrentChannel is not { Id: var channelId }) return;

        await _signalrTextService.SendMessageAsync(message, channelId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IList<MessageModel>> GetMessagePageAsync(ChannelId id, CancellationToken cancellationToken)
    {
        var channel = _channelManager.GetChannel(id);
        if (channel is null or { State.ReachedEnd: true })
        {
            _logger.LogInformation("channel null");

            return Array.Empty<MessageModel>();
        }

        var channelState = channel.State;
        var page = await _messagePageProvider.GetPageAsync(id, channelState.Cursor, cancellationToken).ConfigureAwait(false);

        if (page.Count == 0)
        {
            return Array.Empty<MessageModel>();
        }

        channelState.Cursor = new TextChannelCursor(page[0].Timestamp.UtcTicks);
        return _modelService.CreateModelPage(page, channelState);
    }

    private async Task HandleIncomingMessage(IChatMessage messageDto)
    {
        var channel = _channelManager.GetChannel(messageDto.ChannelId);
        if (channel is null)
        {
            _logger.LogWarning("Received message on nonexistent channel id: {Id}", messageDto.ChannelId);
            return;
        }

        UpdateChannelMessageState(channel, messageDto);

        var channelState = channel.State;
        var message = _modelService.CreateModel(messageDto, channelState, false);

        channelState.AddMessage(message);

        await MessageReceived.TryInvoke(channel.Id, message).ConfigureAwait(false);
    }

    private void UpdateChannelMessageState(ChannelModel channelModel, IChatMessage message)
    {
        var messageByCurrentUser = message.Author.Id == _userAuthenticationService.UserId;

        if (_channelManager.CurrentChannel != channelModel && !messageByCurrentUser)
        {
            channelModel.State.UnreadMessages++;
        }

        channelModel.LastMessage = message;
    }
}
