using Shared.DTOs.Chat;
using Shared.Enums;
using Squadtalk.Data.Entities;

namespace Squadtalk.Data;


public static class Mappers
{
    public static MessageDto ToDto(this Message message)
    {
        return new MessageDto
        {
            Author = message.Author.ToDto(),
            Content = message.Content,
            Timestamp = message.Timestamp,
            ChannelId = message.ChannelId,
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

    public static ChannelDto ToDto(this Channel channel)
    {
        var dto = new ChannelDto
        {
            Id = channel.Id,
            Participants = channel.Participants.Select(x => x.ToDto()).ToList(),
            LastMessage = channel.LastMessage?.ToDto(),
            Name = channel.Name
        };

        return dto;
    }

    public static MessageDto ToDto(this Channel.Message message)
    {
        return new MessageDto
        {
            Author = new UserDto
            {
                Username = message.AuthorName,
                Id = message.AuthorId
            },
            Timestamp = message.Timestamp,
            ChannelId = message.ChannelId,
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