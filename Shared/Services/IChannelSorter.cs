using Shared.Models;

namespace Shared.Services;

public interface IChannelSorter
{
    event Action? ChannelOrderChanged;

    IReadOnlyCollection<ChatModel> SortedChannels { get; }
}
