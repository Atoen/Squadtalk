using Blazored.LocalStorage;
using Shared.Communication;
using Shared.Data;
using Shared.Services;

namespace Squadtalk.Client.Services;

public class ChatVisibilityManager : IChatVisibilityManager
{
    private readonly ITextChatService _textChatService;
    private readonly ILocalStorageService _localStorageService;
    private readonly IMessageService _messageService;
    
    private const string HiddenChats = "hiddenChats";
    
    private HashSet<string> _hiddenChannels = [];
    private readonly List<TextChannel> _visibleChannels = [];

    public event Action? StateChanged;
    
    public IReadOnlyList<TextChannel> VisibleChannels => _visibleChannels;

    private bool _initialized;

    public ChatVisibilityManager(ITextChatService textChatService,
        ILocalStorageService localStorageService, IMessageService messageService)
    {
        _textChatService = textChatService;
        _localStorageService = localStorageService;
        _messageService = messageService;

        _messageService.MessageReceived += MessageReceived;
        _textChatService.StateChangedAsync += UpdateListAsync;
    }

    public async Task UpdateListAsync()
    {
        if (!_initialized)
        {
            await Initialize();
        }
        
        var updatedChannels = _textChatService.AllChannels.Where(x => !_hiddenChannels.Contains(x.Id));
        _visibleChannels.Clear();
        _visibleChannels.AddRange(updatedChannels);
        
        StateChanged?.Invoke();
    }

    private async Task Initialize()
    {
        if (await _localStorageService.ContainKeyAsync(HiddenChats))
        {
            _hiddenChannels = await _localStorageService.GetItemAsync<HashSet<string>>(HiddenChats) ?? [];
        }

        _initialized = true;
    }

    private async Task UpdateLocalStorage()
    {
        await _localStorageService.SetItemAsync(HiddenChats, _hiddenChannels);
        
        await UpdateListAsync();
    }

    private Task MessageReceived(ChannelId id)
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