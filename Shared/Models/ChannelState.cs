using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Reactive;

namespace Shared.Models;

public class ChannelState(ChatModel chat) : Observable<ChannelState>
{
    public TextChannelCursor Cursor { get; set; }
    public bool ScrolledToBeginning { get; set; }

    public List<MessageModel> Messages { get; } = [];
    public TypingUsers TypingUsers { get; } = new();

    public MessageModel? LastMessageReceived { get; set; }
    public MessageModel? LastPageMessageReceived { get; set; }

    public int UnreadMessages { get; set; }

    private bool _hasActiveCall;
    public bool HasActiveCall
    {
        get => _hasActiveCall;
        set => SetField(ref _hasActiveCall, value);
    }

    private MessageModel? _callInfoMessage;

    public void AddMessage(MessageModel message)
    {
        var modifiedExisting = UpdateCallSystemMessage(message);

        if (modifiedExisting) return;

        Messages.Add(message);
        LastMessageReceived = message;

        if (Cursor == default)
        {
            Cursor = new TextChannelCursor(message.Id);
        }
    }

    public void UserIsTyping(UserId userId)
    {
        var typingParticipant = chat.Others.FirstOrDefault(x => x.User.Id == userId);
        if (typingParticipant is null)
        {
            return;
        }

        if (TypingUsers.InsertOrUpdate(typingParticipant.User))
        {
            Notify();
        }
    }

    public void UserStoppedTyping(UserId userId)
    {
        if (TypingUsers.Remove(userId))
        {
            Notify();
        }
    }

    public void RemoveStaleTyping(DateTime now)
    {
        if (TypingUsers.RemoveStale(now))
        {
            Notify();
        }
    }

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
