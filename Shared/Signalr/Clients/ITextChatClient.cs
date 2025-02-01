using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Enums;

namespace Shared.Signalr.Clients;

public interface ITextChatClient
{
    Task ReceivedMessage(MessageDto messageDto);

    Task UserIsTyping(GroupId groupId, UserId userId);

    Task UserStoppedTyping(GroupId groupId, UserId userId);

    Task ReceivedGroups(IList<GroupDto> channelDtos);

    Task AddedToGroup(GroupDto groupDto);

    Task GroupParticipantsChanged(GroupDto groupDto);

    Task ParticipantRoleChanged(GroupId groupId, UserId userId, GroupRole groupRole);

    Task GroupNameChanged(GroupId groupId, string? name);

    Task GroupDeleted(GroupId groupId);
}
