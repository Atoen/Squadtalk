using Blazored.LocalStorage;
using Shared.Communication;
using Shared.Data.TypedIds;
using Shared.Services;

namespace Squadtalk.Client.Services;

public class ChatVisibilityManager : IChatVisibilityManager
{
    private readonly IChatService _chatService;
    private readonly ILocalStorageService _localStorageService;
    private readonly ITextChatService _textChatService;
    
    private const string HiddenChats = "hiddenChats";
    
    private HashSet<ChannelId> _hiddenChannels = [];
    private readonly List<ChannelModel> _visibleChannels = [];

    public event Action? StateChanged;
    
    public IReadOnlyList<ChannelModel> VisibleChannels => _visibleChannels;

    private bool _initialized;

    public ChatVisibilityManager(IChatService chatService,
        ILocalStorageService localStorageService, ITextChatService textChatService)
    {
        _chatService = chatService;
        _localStorageService = localStorageService;
        _textChatService = textChatService;

        _textChatService.MessageReceived += TextChatReceived;
        _chatService.StateChangedAsync += UpdateListAsync;
    }

    public async Task UpdateListAsync()
    {
        if (!_initialized)
        {
            await Initialize();
        }
        
        var updatedChannels = _chatService.AllChannels.Where(x => !_hiddenChannels.Contains(x.Id));
        _visibleChannels.Clear();
        _visibleChannels.AddRange(updatedChannels);
        
        StateChanged?.Invoke();
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

    private Task TextChatReceived(ChannelId id)
    {
        StateChanged?.Invoke();
        
        return StopHidingChannel(id);
    }

    public Task StopHidingChannel(ChannelId id)
    {
        var removed = _hiddenChannels.Remove(id);
        return removed ? UpdateLocalStorage() : Task.CompletedTask;
    }

    public Task HideChannel(ChannelId id)
    {
        var added = _hiddenChannels.Add(id);
        return added ? UpdateLocalStorage() : Task.CompletedTask;
    }
}
