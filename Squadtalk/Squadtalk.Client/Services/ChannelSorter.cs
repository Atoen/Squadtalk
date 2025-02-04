using Shared.Models;
using Shared.Services;

namespace Squadtalk.Client.Services;

internal class ChannelSorter : IChannelSorter
{
    private readonly IChatManager _chatManager;
    private readonly ITextChatService _textChatService;
    private readonly ILogger<ChannelSorter> _logger;

    private List<ChatModel> _channels = [];

    public event Action? ChannelsSorted;

    public IReadOnlyCollection<ChatModel> SortedChannels
    {
        get
        {
            SortChannelsIfNeeded();
            return _channels;
        }
    }

    private bool _shouldSortChannels = true;

    public ChannelSorter(
        IChatManager chatManager,
        ITextChatService textChatService,
        ILogger<ChannelSorter> logger)
    {
        _chatManager = chatManager;
        _textChatService = textChatService;
        _logger = logger;

        _textChatService.MessageReceived += MessageReceived;
        _chatManager.ChatListChanged += OnChatListChanged;
    }

    private void OnChatListChanged()
    {
        _shouldSortChannels = true;
    }

    private void SortChannelsIfNeeded()
    {
        if (!_shouldSortChannels) return;
        _shouldSortChannels = false;

        _channels = _chatManager.Chats.OrderByDescending(x => x.LastMessage?.Timestamp).ToList();

        _logger.LogInformation("Channels sorted");
    }

    private void MessageReceived(ChatModel chat, MessageModel model)
    {
        if (chat.Id != ChatModel.GlobalChatId &&
            _channels is [var first, ..] && first.Id != chat.Id)
        {
            _shouldSortChannels = true;
        }

        ChannelsSorted?.Invoke();
    }
}
