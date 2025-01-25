using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Models;
using Shared.Services;
using Squadtalk.Client.Services.SignalR;

namespace Squadtalk.Client.Services;

internal class TextChatService : ITextChatService
{
    private readonly ILogger<TextChatService> _logger;
    private readonly IMessageModelService _modelService;
    private readonly SignalrService _signalrTextService;
    private readonly IUserAuthenticationService _userAuthenticationService;
    private readonly IChannelManager _channelManager;

    private CancellationTokenSource? _cancellationTokenSource;
    private ChannelId? _typingChannelId;

    public event Action<ChannelId, MessageModel>? MessageReceived;

    public TextChatService(
        IChannelManager channelManager,
        IMessageModelService modelService,
        SignalrService signalrTextService,
        IUserAuthenticationService userAuthenticationService,
        ILogger<TextChatService> logger)
    {
        _channelManager = channelManager;
        _modelService = modelService;
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

    public void StartedTyping(ChannelId channelId)
    {
        if (_typingChannelId == channelId || _typingChannelId == _channelManager.GlobalChat.Id)
        {
            return;
        }

        _typingChannelId = channelId;
        _ = EnterLoop();
    }

    public void StoppedTyping()
    {
        _typingChannelId = null;
        ClearTokenSource();
    }

    public async Task<IList<MessageModel>> GetMessagePageAsync(ChannelId channelId, CancellationToken cancellationToken)
    {
        var channel = _channelManager.GetChannel(channelId);
        if (channel is null or { State.ScrolledToBeginning: true })
        {
            return Array.Empty<MessageModel>();
        }

        var channelState = channel.State;
        var page = await FetchPageAsync(channelId, channelState.Cursor, cancellationToken);

        if (page.Count == 0)
        {
            return Array.Empty<MessageModel>();
        }

        channelState.Cursor = new TextChannelCursor(page[0].Timestamp.UtcTicks);
        return _modelService.CreateModelPage(page, channelState);
    }

    private async Task<IReadOnlyList<MessageDto>> FetchPageAsync(ChannelId channelId, TextChannelCursor cursor, CancellationToken cancellationToken)
    {
        var result = await _signalrTextService.GetMessagePageAsync(channelId, cursor, cancellationToken);
        if (result.IsError)
        {
            _logger.LogError("Failed to fetch message page");
            return [];
        }

        return result.Value;
    }

    private async Task EnterLoop()
    {
        if (_typingChannelId is not { } channelId) return;

        ClearTokenSource();
        _cancellationTokenSource = new CancellationTokenSource();

        var sent = false;

        try
        {
            await Task.Delay(TypingTiming.StartDelayToNotify, _cancellationTokenSource.Token);

            if (_typingChannelId != channelId) return;

            // Setting 'sent' before the loop to avoid skipping the assignment
            // in case an OperationCanceledException is thrown
            sent = true;
            await IsTypingNotificationLoop(channelId, _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
        }

        if (sent)
        {
            await _signalrTextService.UserStoppedTypingAsync(channelId);
        }
    }

    private async Task IsTypingNotificationLoop(ChannelId channelId, CancellationToken cancellationToken)
    {
        while (_typingChannelId == channelId && !cancellationToken.IsCancellationRequested)
        {
            await _signalrTextService.UserIsTypingAsync(channelId, cancellationToken);
            await Task.Delay(TypingTiming.InputBoxInterval, cancellationToken);
        }
    }

    private void HandleIncomingMessage(IChatMessage message)
    {
        var channel = _channelManager.GetChannel(message.ChannelId);
        if (channel is null)
        {
            _logger.LogWarning("Received message on nonexistent channel id: {Id}", message.ChannelId);
            return;
        }

        UpdateChannelMessageState(channel, message);

        var channelState = channel.State;
        var messageModel = _modelService.CreateModel(message, channelState, false);

        channelState.AddMessage(messageModel);

        MessageReceived?.Invoke(channel.Id, messageModel);
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

    private void ClearTokenSource()
    {
        if (_cancellationTokenSource is { } cts)
        {
            cts.Cancel();
            cts.Dispose();
            _cancellationTokenSource = null;
        }
    }
}
