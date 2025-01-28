using Shared.Data.TypedIds;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Client.Services;

internal class ChannelSorter : IChannelSorter
{
    private readonly IChatGroupManager _chatGroupManager;
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
        IChatGroupManager chatGroupManager,
        ITextChatService textChatService,
        ILogger<ChannelSorter> logger)
    {
        _chatGroupManager = chatGroupManager;
        _textChatService = textChatService;
        _logger = logger;

        _textChatService.MessageReceived += MessageReceived;
        _chatGroupManager.ChannelsListChanged += OnChatGroupsListChanged;
    }

    private void OnChatGroupsListChanged()
    {
        _shouldSortChannels = true;
    }

    private void SortChannelsIfNeeded()
    {
        if (!_shouldSortChannels) return;
        _shouldSortChannels = false;

        _channels = _chatGroupManager.Channels.OrderByDescending(x => x.LastMessage?.Timestamp).ToList();

        _logger.LogInformation("Channels sorted");
    }

    private void MessageReceived(GroupId groupId, MessageModel model)
    {
        if (groupId != GroupChatModel.GlobalChatId &&
            _channels is [var first, ..] && first.Id != groupId)
        {
            _shouldSortChannels = true;
        }

        ChannelsSorted?.Invoke();
    }
}
