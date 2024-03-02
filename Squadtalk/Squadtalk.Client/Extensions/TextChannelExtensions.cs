using Shared.Communication;
using Shared.DTOs;

namespace Squadtalk.Client.Extensions;

public static class TextChannelExtensions
{
    public static bool IsGlobal(this TextChannel channel) => channel.Id == GroupChat.GlobalChatId;
    
    public static bool IsFake(this TextChannel channel) => channel.Id == DirectMessageChannel.FakeChannelId;
    
    public static T WithLastMessage<T>(this T textChannel, MessageDto? messageDto, bool byCurrentUser) where T : TextChannel
    {
        if (messageDto is not null)
        {
            textChannel.SetLastMessage(messageDto, byCurrentUser);
        }

        return textChannel;
    }
}