using Shared.Communication;
using Shared.Data;

namespace Squadtalk.Client.Extensions;

public static class TextChannelModelExtensions
{
    public static bool IsGlobal(this ChannelModel? channelModel) => channelModel?.Id == GroupChatModel.GlobalChatId;

    public static bool IsFake(this ChannelModel? channelModel) => channelModel?.Id == DirectMessageChannelModel.FakeChannelId;
    
    public static T WithLastMessage<T>(this T textChannel, IChatMessage? messageDto, bool byCurrentUser) where T : ChannelModel
    {
        if (messageDto is not null)
        {
            textChannel.SetLastMessage(messageDto, byCurrentUser);
        }

        return textChannel;
    }
}
