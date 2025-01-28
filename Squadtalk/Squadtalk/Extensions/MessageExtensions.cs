using Squadtalk.Data.Entities;

namespace Squadtalk.Extensions;

internal static class MessageExtensions
{
    public static Message WithEmbed(this Message message, Embed embed)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.Embed = embed;
        return message;
    }
}