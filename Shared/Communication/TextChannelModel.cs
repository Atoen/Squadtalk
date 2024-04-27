using Shared.Data;
using Shared.Enums;

namespace Shared.Communication;

public abstract class TextChannelModel(ChannelId id)
{
    public abstract string Name { get; }
    
    public string? LastMessage { get; private set; }
    public DateTimeOffset LastMessageTimeStamp { get; private set; }
    
    public ChannelId Id { get; } = id;
    
    public TextChannelState State { get; } = new();

    public void SetLastMessage(string message, DateTimeOffset timestamp, bool byCurrentUser)
    {
        LastMessageTimeStamp = timestamp;
        LastMessage = byCurrentUser ? $"You: {message}" : message;
    }
    
    public void SetLastMessage(IChatMessage message, bool byCurrentUser)
    {
        var contentToDisplay = message.Embed switch
        {
            { Type: EmbedType.File } => "Sent file",
            { Type: EmbedType.Image } => "Sent image",
            { Type: EmbedType.Video } => "Sent video",
            _ => message.Content
        };

        SetLastMessage(contentToDisplay, message.Timestamp, byCurrentUser);
    }
}
