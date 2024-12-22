using Shared.Data.TypedIds;
using Shared.DTOs.Chat;

namespace Squadtalk.Hubs;

public interface ITextChatClient
{
    Task ReceiveMessage(MessageDto messageDto);
    
    Task UserConnected(UserDto userDto);

    Task UserDisconnected(UserDto userDto);

    Task GetConnectedUsers(IList<UserDto> userDtos);

    Task GetChannels(IList<ChannelDto> channelDtos);

    Task AddedToChannel(ChannelDto channelDto);

    Task ChannelNameChanged(ChannelId channelId, string? name);
}