using Shared.DTOs;

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

    public static UserDto ToDto(this ApplicationUser user)
    {
        return new UserDto
        {
            Username = user.UserName!,
            Id = user.Id
        };
    }

    public static ChannelDto ToDto(this Channel channel)
    {
        var dto = new ChannelDto
        {
            Id = channel.Id,
            Participants = channel.Participants.Select(x => x.ToDto()).ToList(),
            LastMessage = channel.LastMessage?.ToDto()
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

    public static VoiceCallDto ToDto(this VoiceCall voiceCall)
    {
        return new VoiceCallDto
        {
            Id = voiceCall.Id,
            Initiator = voiceCall.Initiator.ToDto(),
            Invited = voiceCall.Invited.Select(x => x.ToDto()).ToList(),
            ConnectedIds = voiceCall.ConnectedUsers.Select(x => x.User.ToDto().Id).ToList()
        };
    }
}