using Squadtalk.Data;

namespace Squadtalk.Extensions;

public static class MessageExtensions
{
    public static Message WithEmbed(this Message message, Embed embed)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.Embed = embed;
        return message;
    }
}