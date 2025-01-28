using Shared.Models;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal class ChannelSorter(IChatGroupManager chatGroupManager) : IChannelSorter
{
    event Action? IChannelSorter.ChannelsSorted { add { } remove { } }

    // Channels are already sorted during prerender
    public IReadOnlyCollection<ChatModel> SortedChannels => chatGroupManager.Channels;
}
