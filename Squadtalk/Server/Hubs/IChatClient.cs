using Squadtalk.Shared;

namespace Squadtalk.Server.Hubs;

public interface IChatClient
{
    Task ReceiveMessage(MessageDto message);

    Task UserConnected(UserDto user);

    Task UserDisconnected(UserDto user);

    Task GetConnectedUsers(IEnumerable<UserDto> users);

    Task GetChannels(IEnumerable<ChannelDto> channel);

    Task AddedToChannel(ChannelDto channel);
}