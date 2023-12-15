using Shared.Models;

namespace Shared.Communication;

public class TextChannelState
{
    public long Cursor { get; set; }
    public bool ReachedEnd { get; set; }

    public List<MessageModel> Messages { get; } = [];
    
    public MessageModel? LastMessageReceived { get; set; }
    public MessageModel? LastPageMessageReceived { get; set; }
}