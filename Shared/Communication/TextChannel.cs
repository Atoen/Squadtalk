using Shared.DTOs;
using Shared.Enums;

namespace Shared.Communication;

public abstract class TextChannel(string id)
{
    public abstract string Name { get; }
    
    public string? LastMessage { get; set; }
    public DateTimeOffset LastMessageTimeStamp { get; set; }
    
    public string Id { get; } = id;
    
    public TextChannelState State { get; } = new();

    public void SetLastMessage(string message, DateTimeOffset timestamp, bool byCurrentUser)
    {
        LastMessageTimeStamp = timestamp;
        LastMessage = byCurrentUser ? $"You: {message}" : message;
    }
    
    public void SetLastMessage(MessageDto message, bool byCurrentUser)
    {
        var contentToDisplay = message.Embed switch
        {
            { Type: EmbedType.File } => "Sent file",
            { Type: EmbedType.Image } => "Sent image",
            { Type: EmbedType.Video } => "Sent video",
            _ => message.Content
        };
        
        LastMessageTimeStamp = message.Timestamp;
        LastMessage = byCurrentUser ? $"You: {contentToDisplay}" : contentToDisplay;
    }
}
