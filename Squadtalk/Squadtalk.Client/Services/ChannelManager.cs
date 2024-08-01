using Shared.Data.TypedIds;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Client.Services;

public class ChannelManager : IChannelManager
{
    private readonly IChatService _chatService;
    private readonly ITextChatService _textChatService;
    private readonly ILogger<ChannelManager> _logger;

    private List<ChannelModel> _channels = [];

    public event Action? ChannelsSorted;

    public ICollection<ChannelModel> Channels
    {
        get
        {
            SortChannelsIfNeeded();
            return _channels;
        }
    }

    private bool _shouldSortChannels = true;

    public ChannelManager(
        IChatService chatService,
        ITextChatService textChatService,
        ILogger<ChannelManager> logger)
    {
        _chatService = chatService;
        _textChatService = textChatService;
        _logger = logger;

        _textChatService.MessageReceived += MessageReceived;
        _chatService.ChannelsListChanged += OnChannelsListChanged;
    }

    private void OnChannelsListChanged()
    {
        _shouldSortChannels = true;
    }

    private void SortChannelsIfNeeded()
    {
        if (!_shouldSortChannels) return;
        _shouldSortChannels = false;

        _channels = _chatService.AllChannels.OrderByDescending(x => x.LastMessage?.Timestamp).ToList();

        _logger.LogInformation("Channels sorted");
    }

    private Task MessageReceived(ChannelId channelId)
    {
        if (channelId != GroupChatModel.GlobalChatId &&
            _channels is [var first, ..] && first.Id != channelId)
        {
            _shouldSortChannels = true;
        }

        ChannelsSorted?.Invoke();

        return Task.CompletedTask;
    }
}
