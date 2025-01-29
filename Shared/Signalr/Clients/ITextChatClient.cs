using Shared.Data.TypedIds;
using Shared.DTOs.Chat;

namespace Shared.Signalr.Clients;

public interface ITextChatClient
{
    Task ReceivedMessage(MessageDto messageDto);

    Task UserIsTyping(GroupId groupId, UserId userId);

    Task UserStoppedTyping(GroupId groupId, UserId userId);

    Task ReceivedChannels(IList<GroupDto> channelDtos);

    Task AddedToGroup(GroupDto groupDto);

    Task GroupParticipantsChanged(GroupDto groupDto);

    Task ChannelNameChanged(GroupId groupId, string? name);
}
