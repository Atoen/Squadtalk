using Shared.Data;
using Shared.Enums;

namespace Shared.Models;

public class ChannelState
{
    public TextChannelCursor Cursor { get; set; }
    public bool ScrolledToBeginning { get; set; }

    public List<MessageModel> Messages { get; } = [];
    
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
