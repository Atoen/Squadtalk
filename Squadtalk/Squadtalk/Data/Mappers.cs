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
            Author = message.Author.ToDto(),
            Content = message.Content,
            Timestamp = message.Timestamp,
            GroupId = message.GroupId,
            Embed = message.Embed?.ToDto()
        };
    }

    public static UserDto ToDto(this ApplicationUser user, UserStatus status = UserStatus.Unknown)
    {
        return new UserDto
        {
            Username = user.UserName!,
            Id = user.Id,
            Status = status
        };
    }

    public static UserDto ToDto(this ChatUser user, UserStatus status = UserStatus.Unknown)
    {
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
            AddedBy = addedBy,
            Role = role
        };
    }

    public static GroupParticipantDto ToDto(this GroupParticipant participant)
    {
        return new GroupParticipantDto
        {
            User = participant.User.ToDto(),
            AddedBy = participant.AddedBy?.ToDto(),
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
            Name = group.Name,
            Type = group.ChatType
        };

        return dto;
    }

    public static MessageDto ToDto(this Group.Message message)
    {
        return new MessageDto
        {
            Author = new UserDto
            {
                Username = message.AuthorName,
                Id = message.AuthorId
            },
            Timestamp = message.Timestamp,
            GroupId = message.GroupId,
            Content = message.Content,
            Embed = message.Embed?.ToDto()
        };
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