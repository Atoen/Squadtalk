using Shared.Models;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal class ChannelSorter(IChannelManager channelManager) : IChannelSorter
{
    event Action? IChannelSorter.ChannelsSorted { add { } remove { } }

    // Channels are already sorted during prerender
    public IReadOnlyCollection<ChannelModel> SortedChannels => channelManager.Channels;
}
