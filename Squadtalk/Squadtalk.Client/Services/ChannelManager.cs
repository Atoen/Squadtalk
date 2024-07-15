using Blazored.LocalStorage;
using Shared.Communication;
using Shared.Data.TypedIds;
using Shared.Services;

namespace Squadtalk.Client.Services;

public class ChannelManager : IChannelManager
{
    private readonly IChatService _chatService;
    private readonly ILocalStorageService _localStorageService;
    private readonly ITextChatService _textChatService;
    private readonly ILogger<ChannelManager> _logger;

    private const string HiddenChats = "hiddenChats";

    private HashSet<ChannelId> _hiddenChannels = [];
    private List<ChannelModel> _visibleChannels = [];

    public event Action? ChannelListChanged;

    public IEnumerable<ChannelModel> VisibleChannels
    {
        get
        {
            SortChannelsIfNeeded();
            return _visibleChannels;
        }
    }

    private bool _shouldSortChannels = true;
    private bool _initialized;

    public ChannelManager(
        IChatService chatService,
        ILocalStorageService localStorageService,
        ITextChatService textChatService,
        ILogger<ChannelManager> logger)
    {
        _chatService = chatService;
        _localStorageService = localStorageService;
        _textChatService = textChatService;
        _logger = logger;

        _textChatService.MessageReceived += MessageReceived;
        _chatService.StateChangedAsync += UpdateListAsync;
    }

    public Task StopHidingChannel(ChannelId channelId)
    {
        var removed = _hiddenChannels.Remove(channelId);
        return removed ? UpdateLocalStorage() : Task.CompletedTask;
    }

    public Task HideChannel(ChannelId channelId)
    {
        var added = _hiddenChannels.Add(channelId);
        return added ? UpdateLocalStorage() : Task.CompletedTask;
    }

    public async Task UpdateListAsync()
    {
        if (!_initialized)
        {
            await Initialize();
        }

        _visibleChannels = _chatService.AllChannels.Where(x => !_hiddenChannels.Contains(x.Id)).ToList();
        _shouldSortChannels = true;

        ChannelListChanged?.Invoke();
    }

    private void SortChannelsIfNeeded()
    {
        if (!_shouldSortChannels) return;
        _shouldSortChannels = false;

        _visibleChannels = _visibleChannels.OrderByDescending(x => x.LastMessageTimeStamp).ToList();

        _logger.LogInformation("Channels sorted");
    }

    private async Task Initialize()
    {
        if (await _localStorageService.ContainKeyAsync(HiddenChats))
        {
            _hiddenChannels = await _localStorageService.GetItemAsync<HashSet<ChannelId>>(HiddenChats) ?? [];
        }

        _initialized = true;
    }

    private async Task UpdateLocalStorage()
    {
        await _localStorageService.SetItemAsync(HiddenChats, _hiddenChannels);

        await UpdateListAsync();
    }

    private async Task MessageReceived(ChannelId channelId)
    {
        if (_hiddenChannels.Contains(channelId))
        {
            await StopHidingChannel(channelId);
        }

        if (_visibleChannels is [var first, ..] && first.Id == channelId)
        {
            return;
        }

        _shouldSortChannels = true;

        ChannelListChanged?.Invoke();
    }
}
