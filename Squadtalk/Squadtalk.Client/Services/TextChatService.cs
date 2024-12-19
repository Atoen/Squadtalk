using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs;
using Shared.Extensions;
using Shared.Models;
using Shared.Services;
using Squadtalk.Client.Network;
using Squadtalk.Client.SignalR;

namespace Squadtalk.Client.Services;

internal class TextChatService : ITextChatService
{
    private readonly ILogger<TextChatService> _logger;
    private readonly IMessageModelService _modelService;
    private readonly ISignalrTextService _signalrTextService;
    private readonly IMessageApi _messageApi;
    private readonly IUserAuthenticationService _userAuthenticationService;
    private readonly IChannelManager _channelManager;

    public event Func<ChannelId, MessageModel, Task>? MessageReceived;

    public TextChatService(
        IChannelManager channelManager,
        IMessageModelService modelService,
        SignalrService signalrTextService,
        IMessageApi messageApi,
        IUserAuthenticationService userAuthenticationService,
        ILogger<TextChatService> logger)
    {
        _channelManager = channelManager;
        _modelService = modelService;
        _signalrTextService = signalrTextService;
        _messageApi = messageApi;
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
            return Array.Empty<MessageModel>();
        }

        var channelState = channel.State;
        var page = await FetchPageAsync(id, channelState.Cursor, cancellationToken);

        channelState.Cursor = new TextChannelCursor(page[0].Timestamp.UtcTicks);
        return _modelService.CreateModelPage(page, channelState);
    }

    private async Task<IReadOnlyList<MessageDto>> FetchPageAsync(ChannelId channelId, TextChannelCursor cursor, CancellationToken cancellationToken)
    {
        try
        {
            return await _messageApi.GetMessagePage(channelId, cursor, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while fetching message page");
            return Array.Empty<MessageDto>();
        }
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
