using Blazored.LocalStorage;
using Shared.Communication;
using Shared.Services;

namespace Squadtalk.Client.Services;

public class ChatVisibilityManager : IChatVisibilityManager
{
    private readonly ICommunicationManager _communicationManager;
    private readonly ILocalStorageService _localStorageService;
    private readonly IMessageService _messageService;
    
    private const string HiddenChats = "hiddenChats";
    
    private HashSet<string> _hiddenChannels = [];
    private readonly List<TextChannel> _visibleChannels = [];

    public event Action? StateChanged;
    
    public IReadOnlyList<TextChannel> VisibleChannels => _visibleChannels;

    private bool _initialized;

    public ChatVisibilityManager(ICommunicationManager communicationManager,
        ILocalStorageService localStorageService, IMessageService messageService)
    {
        _communicationManager = communicationManager;
        _localStorageService = localStorageService;
        _messageService = messageService;

        _messageService.MessageReceived += MessageReceived;
        _communicationManager.StateChangedAsync += UpdateListAsync;
    }

    public async Task UpdateListAsync()
    {
        if (!_initialized)
        {
            await Initialize();
        }
        
        var updatedChannels = _communicationManager.AllChannels.Where(x => !_hiddenChannels.Contains(x.Id));
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

    private Task MessageReceived(string channelId)
    {
        StateChanged?.Invoke();
        
        return StopHidingChannel(channelId);
    }

    public Task StopHidingChannel(string channelId)
    {
        var removed = _hiddenChannels.Remove(channelId);
        return removed ? UpdateLocalStorage() : Task.CompletedTask;
    }

    public Task HideChannel(string channelId)
    {
        var added = _hiddenChannels.Add(channelId);
        return added ? UpdateLocalStorage() : Task.CompletedTask;
    }
}