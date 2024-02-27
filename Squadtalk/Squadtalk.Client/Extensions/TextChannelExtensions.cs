using Shared.Communication;

namespace Squadtalk.Client.Extensions;

public static class TextChannelExtensions
{
    public static bool IsGlobal(this TextChannel channel) => channel.Id == GroupChat.GlobalChatId;
    
    public static bool IsFake(this TextChannel channel) => channel.Id == DirectMessageChannel.FakeChannelId;
}