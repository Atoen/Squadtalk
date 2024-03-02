using Squadtalk.Data;

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
}