namespace Squadtalk.Client.Models.Communication;

public static class ChannelExtensions
{
    public static bool IsGlobal(this Channel channel) => channel.Id == GroupChat.GlobalChatId;
    public static bool IsFake(this Channel channel) => channel.Id == DirectMessageChannel.FakeChannelId;
}