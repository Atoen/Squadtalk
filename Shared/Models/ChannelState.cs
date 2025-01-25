using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;

namespace Shared.Models;

public class ChannelState(ChannelModel channel)
{
    public TextChannelCursor Cursor { get; set; }
    public bool ScrolledToBeginning { get; set; }

    public List<MessageModel> Messages { get; } = [];
    public TypingUsers TypingUsers { get; } = new();

    public MessageModel? LastMessageReceived { get; set; }
    public MessageModel? LastPageMessageReceived { get; set; }

    public int UnreadMessages { get; set; }

    public bool HasActiveCall { get; set; }

    private MessageModel? _callInfoMessage;

    public void AddMessage(MessageModel message)
    {
        var modifiedExisting = UpdateCallSystemMessage(message);

        if (modifiedExisting) return;

        Messages.Add(message);
        LastMessageReceived = message;

        if (Cursor == default)
        {
            Cursor = TextChannelCursor.New;
        }
    }

    public bool UserIsTyping(UserId userId)
    {
        var typingUser = channel.Others.FirstOrDefault(x => x.Id == userId);
        if (typingUser is null)
        {
            return false;
        }

        return TypingUsers.InsertOrUpdate(typingUser);
    }

    public bool UserStoppedTyping(UserId userId) => TypingUsers.Remove(userId);

    public bool RemoveStaleTyping(DateTime now) => TypingUsers.RemoveStale(now);

    private bool UpdateCallSystemMessage(MessageModel message)
    {
        if (!message.IsSystemMessage) return false;

        var systemMessageType = SystemMessageTypeHelper.Parse(message.Embed[EmbedData.SystemMessageType]);
        if (systemMessageType == SystemMessageType.CallStarted)
        {
            _callInfoMessage = message;
        }
        else if (systemMessageType is SystemMessageType.CallEnded or SystemMessageType.CallMissed)
        {
            if (_callInfoMessage is null) return false;

            var index = Messages.IndexOf(_callInfoMessage);
            Messages[index] = message;
            Messages[index].Timestamp = _callInfoMessage.Timestamp;

            _callInfoMessage = null;
            return true;
        }

        return false;
    }
}
