using Shared.Communication;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Services;

public class ServersideTabManager : ITabManager
{
    public event Action? StateChanged;
    public IReadOnlyList<GroupChat> VisibleGroupChats { get; } = [];
    public IReadOnlyList<UserModel> VisibleUsers { get; } = [];
    public Task ShowHiddenTab(string tabId)
    {
        // throw new NotImplementedException();
        return Task.CompletedTask;

    }

    public Task HideTab(string tabId)
    {
        // throw new NotImplementedException();
        return Task.CompletedTask;
    }
}