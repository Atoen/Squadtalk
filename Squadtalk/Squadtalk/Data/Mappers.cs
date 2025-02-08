using Shared.Data;
using Shared.DTOs.Chat;
using Shared.Enums;
using Squadtalk.Data.Entities;

namespace Squadtalk.Data;

internal static class Mappers
{
    public static MessageDto ToDto(this Message message)
    {
        return new MessageDto
        {
            Id = message.Id,
            Author = message.Author.ToDto(),
            Content = message.Content,
            Timestamp = message.Timestamp,
            GroupId = message.GroupId,
            Embed = message.Embed?.ToDto()
        };
    }

    public static UserDto ToDto(this IChatUser user, UserStatus status = UserStatus.Unknown)
    {
        if (user is UserDto dto)
        {
            return dto;
        }
        
        return new UserDto
        {
            Username = user.Username,
            Id = user.Id,
            Status = status
        };
    }

    public static GroupParticipant ToGroupParticipant(this ChatUser user, Group group, ChatUser? addedBy = null)
    {
        var role = group.GroupCreator.Id == user.Id ? GroupRole.Owner : GroupRole.Member;
        return new GroupParticipant
        {
            User = user,
            UserId = user.Id,
            Group = group,
            GroupId = group.Id,
            AddedBy = addedBy ?? throw new ArgumentNullException(nameof(addedBy)),
            Role = role
        };
    }

    public static GroupParticipantDto ToDto(this GroupParticipant participant)
    {
        return new GroupParticipantDto
        {
            User = participant.User.ToDto(),
            AddedBy = participant.AddedBy.ToDto(),
            Role = participant.Role
        };
    }

    public static GroupDto ToDto(this Group group)
    {
        var dto = new GroupDto
        {
            Id = group.Id,
            Participants = group.Participants.Select(x => x.ToDto()).ToList(),
            LastMessage = group.LastMessage?.ToDto(),
            CustomName = group.CustomName,
            Type = group.ChatType
        };

        return dto;
    }

    public static EmbedDto ToDto(this Embed embed)
    {
        return new EmbedDto
        {
            Type = embed.Type,
            Data = embed.Data
        };
    }

    public static PendingFriendRequestDto ToDto(this FriendRequest friendRequest)
    {
        if (friendRequest.IsAccepted is not null)
        {
            throw new InvalidOperationException("Friend request is not pending");
        }

        return new PendingFriendRequestDto
        {
            Id = friendRequest.Id,
            Recipient = friendRequest.Recipient.ToDto(),
            Requester = friendRequest.Requester.ToDto(),
            CreatedAt = friendRequest.CreatedAt
        };
    }
}