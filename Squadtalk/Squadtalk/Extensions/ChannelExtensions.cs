using System.Security.Claims;
using Shared.Data.TypedIds;
using Shared.Extensions;
using Squadtalk.Data.Entities;

namespace Squadtalk.Extensions;

public static class ChannelExtensions
{
    public static Channel WithLastMessage(this Channel textChannel, Message message)
    {
        textChannel.LastMessage = new Channel.Message
        {
            Content = message.Content,
            AuthorId = message.Author.Id,
            AuthorName = message.Author.UserName!,
            ChannelId = message.ChannelId,
            Timestamp = message.Timestamp,
            Embed = message.Embed is not null
                ? new Embed { Type = message.Embed.Type, Data = message.Embed.Data }
                : null
        };

        return textChannel;
    }

    public static bool UserParticipatesInChannel(this Channel channel, ClaimsPrincipal? principal)
    {
        if (principal?.GetClaimValue(ClaimTypes.NameIdentifier) is not { } claim)
        {
            return false;
        }

        return UserId.TryParse(claim, out var userId) && channel.UserParticipatesInChannel(userId);
    }

    public static bool UserParticipatesInChannel(this Channel channel, UserId userId)
    {
        return channel.Participants.Any(x => x.Id == userId);
    }
}
