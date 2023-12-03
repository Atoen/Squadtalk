using Shared.DTOs;

namespace Squadtalk.Hubs;

public interface IChatClient
{
    Task ReceiveMessage(MessageDto messageDto);
    
    Task UserConnected(UserDto userDto);

    Task UserDisconnected(UserDto userDto);

    Task GetConnectedUsers(IEnumerable<UserDto> userDtos);

    Task GetChannels(IEnumerable<ChannelDto> channelDtos);

    Task AddedToChannel(ChannelDto channelDto);
}