using Shared.Communication;

namespace Shared.Services;

public interface IChatVisibilityManager
{
    event Action? StateChanged;
    
    IReadOnlyList<TextChannel> VisibleChannels { get; }

    Task StopHidingChannel(string channelId);

    Task HideChannel(string channelId);

    Task UpdateListAsync();
}