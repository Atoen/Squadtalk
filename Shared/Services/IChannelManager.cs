using Shared.Communication;
using Shared.Data.TypedIds;

namespace Shared.Services;

public interface IChannelManager
{
    event Action? ChannelListChanged;

    IEnumerable<ChannelModel> VisibleChannels { get; }

    Task StopHidingChannel(ChannelId channelId);

    Task HideChannel(ChannelId channelId);

    Task UpdateListAsync();
}
