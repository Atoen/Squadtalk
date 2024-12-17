using Shared.Data;
using Shared.Models;

namespace Shared.Extensions;

public static class ChannelModelExtensions
{
    public static bool IsGlobal(this ChannelModel? channelModel)
    {
        return channelModel?.Id == GroupChatModel.GlobalChatId;
    }

    public static bool IsTemporary(this ChannelModel? channelModel)
    {
        return channelModel?.Id.Value.StartsWith(DirectMessageChannelModel.TempChannelIdPrefix) ?? false;
    }

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
