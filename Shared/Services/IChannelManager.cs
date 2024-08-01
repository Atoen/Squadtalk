using Shared.Models;

namespace Shared.Services;

public interface IChannelManager
{
    event Action? ChannelsSorted;

    ICollection<ChannelModel> Channels { get; }
}
