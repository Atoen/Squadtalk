using Shared.Data;
using Shared.Models;

namespace Shared.Extensions;

public static class ChatModelExtensions
{
    public static bool IsGlobal(this ChatModel? chatModel)
    {
        return chatModel?.Id == ChatModel.GlobalChatId;
    }

    public static bool IsTemporary(this ChatModel? chatModel)
    {
        return chatModel is DirectMessageModel { IsTemporary: true };
    }

    public static bool IsNullOrSpecial(this ChatModel? chatModel)
    {
        return chatModel is null || chatModel.IsGlobal() || chatModel.IsTemporary();
    }

    public static int UnreadMessages(this ChatModel chatModel) => chatModel.State.UnreadMessages;

    public static bool HasUnreadMessages(this ChatModel chatModel) => chatModel.State.UnreadMessages != 0;

    public static bool HasActiveCall(this ChatModel chatModel) => chatModel.State.HasActiveCall;

    public static bool IsSomeoneTyping(this ChatModel chatModel) => chatModel.State.TypingUsers.Count != 0;

    public static IReadOnlyCollection<TypingUser> TypingUsers(this ChatModel chatModel) => chatModel.State.TypingUsers.Typing;

    public static T WithLastMessage<T>(this T textChannel, IChatMessage? message) where T : ChatModel
    {
        textChannel.LastMessage = message;
        return textChannel;
    }

    public static T WithUnreadMessageCount<T>(this T textChannel, int unreadMessageCount) where T : ChatModel
    {
        textChannel.State.UnreadMessages = unreadMessageCount;
        return textChannel;
    }
}
