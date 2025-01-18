using Shared.Data.TypedIds;
using Shared.DTOs.Chat;

namespace Shared.Signalr.Clients;

public interface ITextChatClient
{
    Task ReceivedMessage(MessageDto messageDto);

    Task UserIsTyping(ChannelId channelId, UserId userId);

    Task UserStoppedTyping(ChannelId channelId, UserId userId);

    Task ReceivedChannels(IList<ChannelDto> channelDtos);

    Task AddedToChannel(ChannelDto channelDto);

    Task ChannelNameChanged(ChannelId channelId, string? name);
}
