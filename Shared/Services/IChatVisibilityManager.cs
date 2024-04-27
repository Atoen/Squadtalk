using Shared.Communication;
using Shared.Data;

namespace Shared.Services;

public interface IChatVisibilityManager
{
    event Action? StateChanged;
    
    IReadOnlyList<TextChannelModel> VisibleChannels { get; }

    Task StopHidingChannel(ChannelId id);

    Task HideChannel(ChannelId id);

    Task UpdateListAsync();
}