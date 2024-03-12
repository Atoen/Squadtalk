using Shared.Data;
using Shared.DTOs;
using Shared.Enums;

namespace Shared.Communication;

public abstract class TextChannel(ChannelId id)
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
    
    public void SetLastMessage(MessageDto message, bool byCurrentUser)
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
