using Shared.Data.TypedIds;
using Shared.Models;

namespace Shared.Services;

public interface IChannelManager
{
    event Action? ChannelListChanged;

    ICollection<ChannelModel> VisibleChannels { get; }

    Task StopHidingChannel(ChannelId channelId);

    Task HideChannel(ChannelId channelId);

    Task UpdateListAsync();

    Task InitializeAsync();
}
