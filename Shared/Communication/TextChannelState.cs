using Shared.Data;
using Shared.Models;

namespace Shared.Communication;

public class TextChannelState
{
    public TextChannelCursor Cursor { get; set; }
    public bool ReachedEnd { get; set; }

    public List<MessageModel> Messages { get; } = [];
    
    public MessageModel? LastMessageReceived { get; set; }
    public MessageModel? LastPageMessageReceived { get; set; }
    
    public int UnreadMessages { get; set; }
}