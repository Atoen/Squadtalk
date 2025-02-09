using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Enums;
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
    private readonly IChatManager _chatManager;

    private CancellationTokenSource? _cancellationTokenSource;
    private GroupId? _typingChannelId;

    public event Action<ChatModel, MessageModel>? MessageReceived;

    public TextChatService(
        IChatManager chatManager,
        IMessageModelService modelService,
        SignalrService signalrTextService,
        IUserAuthenticationService userAuthenticationService,
        ILogger<TextChatService> logger)
    {
        _chatManager = chatManager;
        _modelService = modelService;
        _signalrTextService = signalrTextService;
        _userAuthenticationService = userAuthenticationService;
        _logger = logger;

        _signalrTextService.MessageReceived += HandleIncomingMessage;
    }

    public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        if (_chatManager.CurrentChat is not { Id: var channelId }) return;

        _logger.LogInformation("Sending message: {Text} on channel: {Id}", message, channelId);

        await _signalrTextService.SendMessageAsync(message, channelId, cancellationToken);
    }

    public void StartedTyping(GroupId groupId)
    {
        if (_typingChannelId == groupId || _typingChannelId == _chatManager.GlobalChat.Id)
        {
            return;
        }

        if (_signalrTextService.UserStatus == UserStatus.Offline)
        {
            return;
        }

        _typingChannelId = groupId;
        _ = EnterLoop();
    }

    public void StoppedTyping()
    {
        _typingChannelId = null;
        ClearTokenSource();
    }

    public async Task<IList<MessageModel>> GetMessagePageAsync(GroupId groupId, CancellationToken cancellationToken)
    {
        var channel = _chatManager.GetChannel(groupId);
        if (channel is null or { State.ScrolledToBeginning: true })
        {
            return Array.Empty<MessageModel>();
        }

        var channelState = channel.State;
        var page = await FetchPageAsync(groupId, channelState.Cursor, cancellationToken);

        if (page.Count == 0)
        {
            return Array.Empty<MessageModel>();
        }

        channelState.Cursor = new TextChannelCursor(page[0].Id);
        return _modelService.CreateModelPage(page, channelState);
    }

    private async Task<IReadOnlyList<MessageDto>> FetchPageAsync(GroupId groupId, TextChannelCursor cursor, CancellationToken cancellationToken)
    {
        var result = await _signalrTextService.GetMessagePageAsync(groupId, cursor, cancellationToken);
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

    private async Task IsTypingNotificationLoop(GroupId groupId, CancellationToken cancellationToken)
    {
        while (_typingChannelId == groupId && !cancellationToken.IsCancellationRequested)
        {
            await _signalrTextService.UserIsTypingAsync(groupId, cancellationToken);
            await Task.Delay(TypingTiming.InputBoxInterval, cancellationToken);
        }
    }

    private void HandleIncomingMessage(IChatMessage message)
    {
        var channel = _chatManager.GetChannel(message.GroupId);
        if (channel is null)
        {
            _logger.LogWarning("Received message on nonexistent channel id: {Id}", message.GroupId);
            return;
        }

        UpdateChannelMessageState(channel, message);

        var channelState = channel.State;
        var messageModel = _modelService.CreateModel(message, channelState, false);

        channelState.AddMessage(messageModel);

        MessageReceived?.Invoke(channel, messageModel);
    }

    private void UpdateChannelMessageState(ChatModel chatModel, IChatMessage message)
    {
        var messageByCurrentUser = message.Author.Id == _userAuthenticationService.UserId;
        if (_chatManager.CurrentChat != chatModel && !messageByCurrentUser)
        {
            chatModel.State.UnreadMessages++;
        }

        chatModel.LastMessage = message;
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
