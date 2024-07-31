using Shared.Data;
using Shared.Models;
using Squadtalk.Client.Localization;

namespace Squadtalk.Client.Extensions;

public static class ChannelModelExtensions
{
    public static bool IsGlobal(this ChannelModel? channelModel) => channelModel?.Id == GroupChatModel.GlobalChatId;

    public static bool IsFake(this ChannelModel? channelModel) => channelModel?.Id == DirectMessageChannelModel.FakeChannelId;

    public static T WithLastMessage<T>(this T textChannel, IChatMessage? message) where T : ChannelModel
    {
        if (message is null)
        {
            return textChannel;
        }

        textChannel.LastMessage = message;

        return textChannel;
    }

    public static T WithUnreadMessageCount<T>(this T textChannel, int unreadMessageCount) where T : ChannelModel
    {
        textChannel.State.UnreadMessages = unreadMessageCount;

        return textChannel;
    }
}
