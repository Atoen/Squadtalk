using Shared.Data.TypedIds;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Client.Services;

internal class ChannelSorter : IChannelSorter
{
    private readonly IChannelManager _channelManager;
    private readonly ITextChatService _textChatService;
    private readonly ILogger<ChannelSorter> _logger;

    private List<ChannelModel> _channels = [];

    public event Action? ChannelsSorted;

    public IReadOnlyCollection<ChannelModel> SortedChannels
    {
        get
        {
            SortChannelsIfNeeded();
            return _channels;
        }
    }

    private bool _shouldSortChannels = true;

    public ChannelSorter(
        IChannelManager channelManager,
        ITextChatService textChatService,
        ILogger<ChannelSorter> logger)
    {
        _channelManager = channelManager;
        _textChatService = textChatService;
        _logger = logger;

        _textChatService.MessageReceived += MessageReceived;
        _channelManager.ChannelsListChanged += OnChannelsListChanged;
    }

    private void OnChannelsListChanged()
    {
        _shouldSortChannels = true;
    }

    private void SortChannelsIfNeeded()
    {
        if (!_shouldSortChannels) return;
        _shouldSortChannels = false;

        _channels = _channelManager.Channels.OrderByDescending(x => x.LastMessage?.Timestamp).ToList();

        _logger.LogInformation("Channels sorted");
    }

    private void MessageReceived(ChannelId channelId, MessageModel model)
    {
        if (channelId != GroupChatModel.GlobalChatId &&
            _channels is [var first, ..] && first.Id != channelId)
        {
            _shouldSortChannels = true;
        }

        ChannelsSorted?.Invoke();
    }
}
