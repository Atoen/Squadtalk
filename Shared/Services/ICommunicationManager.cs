using Shared.Communication;

namespace Shared.Services;

public interface ICommunicationManager
{
    TextChannel? GetChannel(string channelId);
    TextChannel CurrentChannel { get; }

    TextChannelState CurrentChannelState => CurrentChannel.State;

    event Action ChannelChanged;

    event Action StateChanged;
}