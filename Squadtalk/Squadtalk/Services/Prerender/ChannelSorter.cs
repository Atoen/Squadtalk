using Shared.Models;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal class ChannelSorter(IChatManager chatManager) : IChannelSorter
{
    event Action? IChannelSorter.ChannelOrderChanged { add { } remove { } }

    // Channels are already sorted during prerender
    public IReadOnlyCollection<ChatModel> SortedChannels => chatManager.Chats;
}
