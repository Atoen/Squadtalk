using Blazored.LocalStorage;
using Shared.Communication;
using Shared.Enums;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Client.Services;

public class TabManager : ITabManager
{
    private readonly ICommunicationManager _communicationManager;
    private readonly ILocalStorageService _localStorageService;
    private readonly IMessageService _messageService;

    private const string HiddenUsers = "hiddenDM";
    private const string HiddenGroupChats = "hiddenChats";
    
    private List<string> _hiddenGroupChats = [];
    private List<string> _hiddenUsers = [];
    
    private readonly List<UserModel> _visibleUsers = [];
    private readonly List<GroupChat> _visibleGroupChats = [];

    public event Action? StateChanged;

    public IReadOnlyList<UserModel> VisibleUsers => _visibleUsers;

    public IReadOnlyList<GroupChat> VisibleGroupChats => _visibleGroupChats;

    private bool _initialized;

    public TabManager(ICommunicationManager communicationManager, ILocalStorageService 
            localStorageService, IMessageService messageService)
    {
        _communicationManager = communicationManager;
        _localStorageService = localStorageService;
        _messageService = messageService;

        _messageService.MessageReceived += MessageReceived;
        _communicationManager.StateChangedAsync += UpdateLists;
    }

    private async Task UpdateLists()
    {
        if (!_initialized)
        {
            await Initialize();
        }
        
        var updatedUsers = _communicationManager.Users
            .Where(x => x.Status != UserStatus.Offline || !_hiddenUsers.Contains(x.Id));
        
        _visibleUsers.Clear();
        _visibleUsers.AddRange(updatedUsers);
        
        var updatedUGroupChats = _communicationManager.GroupChats.Where(x => !_hiddenGroupChats.Contains(x.Id));
        _visibleGroupChats.Clear();
        _visibleGroupChats.AddRange(updatedUGroupChats);
        
        StateChanged?.Invoke();
    }

    private async Task Initialize()
    {
        if (await _localStorageService.ContainKeyAsync(HiddenUsers))
        {
            _hiddenUsers = await _localStorageService.GetItemAsync<List<string>>(HiddenUsers);
        }
        
        if (await _localStorageService.ContainKeyAsync(HiddenGroupChats))
        {
            _hiddenGroupChats = await _localStorageService.GetItemAsync<List<string>>(HiddenGroupChats);
        }

        _initialized = true;
    }

    private async Task UpdateLocalStorage()
    {
        await _localStorageService.SetItemAsync(HiddenUsers, _hiddenUsers);
        await _localStorageService.SetItemAsync(HiddenGroupChats, _hiddenGroupChats);
        
        await UpdateLists();
    }

    private Task MessageReceived(string channelId)
    {
        StateChanged?.Invoke();
        
        return ShowHiddenTab(channelId);
    }

    public Task ShowHiddenTab(string tabId)
    {
        var channel = _communicationManager.GetChannel(tabId);

        if (channel is DirectMessageChannel directMessageChannel)
        {
            var other = directMessageChannel.Other;
            var removed = _hiddenUsers.Remove(other.Id);

            if (removed)
            {
                return UpdateLocalStorage();
            }
        }

        else if (channel is GroupChat groupChat)
        {
            var removed = _hiddenGroupChats.Remove(groupChat.Id);
            if (removed)
            {
                return UpdateLocalStorage();
            }
        }

        return Task.CompletedTask;
    }

    public Task HideTab(string tabId)
    {
        var channel = _communicationManager.GetChannel(tabId);
        if (channel is DirectMessageChannel directMessageChannel)
        {
            var other = directMessageChannel.Other;
            _hiddenUsers.Add(other.Id);

            return UpdateLocalStorage();
        }

        if (channel is GroupChat groupChat)
        {
            _hiddenGroupChats.Add(groupChat.Id);
            return UpdateLocalStorage();
        }
        
        return Task.CompletedTask;
    }
}