using Shared.Communication;
using Shared.Models;

namespace Shared.Services;

public interface ITabManager
{
    event Action? StateChanged;
    
    IReadOnlyList<GroupChat> VisibleGroupChats { get; }
    
    IReadOnlyList<UserModel> VisibleUsers { get; }

    Task ShowHiddenTab(string tabId);

    Task HideTab(string tabId);
}