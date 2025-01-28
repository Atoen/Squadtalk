using Shared.Models;

namespace Shared.Services;

public interface IChannelSorter
{
    event Action? ChannelsSorted;

    IReadOnlyCollection<ChatModel> SortedChannels { get; }
}
