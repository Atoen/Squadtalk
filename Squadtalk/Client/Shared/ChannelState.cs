using Squadtalk.Client.Models;

namespace Squadtalk.Client.Shared;

public class ChannelState
{
    public long Cursor { get; set; }
    public bool ReachedEnd { get; set; }

    public List<MessageModel> Messages { get; } = new();
    public MessageModel? LastMessageReceived { get; set; }
    public MessageModel? LastMessageFormatted { get; set; }
}