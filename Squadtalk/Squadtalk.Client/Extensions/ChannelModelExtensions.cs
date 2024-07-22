using Shared.Data;
using Shared.Models;
using Squadtalk.Client.Localization;

namespace Squadtalk.Client.Extensions;

public static class ChannelModelExtensions
{
    public static bool IsGlobal(this ChannelModel? channelModel) => channelModel?.Id == GroupChatModel.GlobalChatId;

    public static bool IsFake(this ChannelModel? channelModel) => channelModel?.Id == DirectMessageChannelModel.FakeChannelId;

    // public static void SetLocalizedLastMessage(this ChannelModel textChannel, IChatMessage message, TextTable textTable, bool byCurrentUser)
    // {
    //     var contentToDisplay = message.Embed switch
    //     {
    //         { Type: EmbedType.File } =>  $"{(byCurrentUser ? textTable.MessageYouSentInfo : textTable.MessageSentInfo)} {textTable.File}",
    //         { Type: EmbedType.Image } => $"{(byCurrentUser ? textTable.MessageYouSentInfo : textTable.MessageSentInfo)} {textTable.Image}",
    //         { Type: EmbedType.Video } => $"{(byCurrentUser ? textTable.MessageYouSentInfo : textTable.MessageSentInfo)} {textTable.Video}",
    //         { Type: EmbedType.SystemMessage } => "SystemMessage",
    //         _ => message.Content
    //     };
    //
    //     var isSystemMessage = message.Embed?.Type == EmbedType.SystemMessage;
    //
    //     var authorPrefix = textChannel switch
    //     {
    //         _ when isSystemMessage => string.Empty,
    //         _ when byCurrentUser => textTable.You,
    //         DirectMessageChannelModel => string.Empty,
    //         _ => message.Author.Username
    //     };
    //
    //     textChannel.SetLastMessage(contentToDisplay, message.Timestamp, authorPrefix);
    // }

    public static T WithLastMessage<T>(this T textChannel, IChatMessage? message, TextTable textTable, bool byCurrentUser) where T : ChannelModel
    {
        if (message is null)
        {
            return textChannel;
        }

        textChannel.SetLastMessage(message);

        return textChannel;
    }
}
