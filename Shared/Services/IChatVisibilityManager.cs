using Shared.Communication;
using Shared.Data.TypedIds;

namespace Shared.Services;

public interface IChatVisibilityManager
{
    event Action? StateChanged;
    
    IEnumerable<ChannelModel> VisibleChannels { get; }

    Task StopHidingChannel(ChannelId id);

    Task HideChannel(ChannelId id);

    Task UpdateListAsync();
}
