using Shared.Data;
using Shared.Data.TypedIds;

namespace Shared.Models;

public abstract class ChannelModel(ChannelId id)
{
    public abstract string Name { get; }
    
    public IChatMessage? LastMessage { get; private set; }
    // public DateTimeOffset LastMessageTimeStamp { get; private set; }
    
    public ChannelId Id { get; } = id;
    
    public ChannelState State { get; } = new();
    
    // public void SetLastMessage(IChatMessage message, bool byCurrentUser)
    // {
    //     var contentToDisplay = message.Embed switch
    //     {
    //         { Type: EmbedType.File } => "Sent file",
    //         { Type: EmbedType.Image } => "Sent image",
    //         { Type: EmbedType.Video } => "Sent video",
    //         { Type: EmbedType.SystemMessage } => "SystemMessage",
    //         _ => message.Content
    //     };
    //
    //     var isSystemMessage = message.Embed?.Type == EmbedType.SystemMessage;
    //
    //     var authorPrefix = this switch
    //     {
    //         _ when isSystemMessage => string.Empty,
    //         _ when byCurrentUser => CurrentUserAuthorPrefix,
    //         DirectMessageChannelModel => string.Empty,
    //         _ => message.Author.Username
    //     };
    //
    //     SetLastMessage(contentToDisplay, message.Timestamp, authorPrefix);
    // }

    public void SetLastMessage(IChatMessage message)
    {
        LastMessage = message;
        // LastMessageTimeStamp = timestamp;
        // LastMessage = string.IsNullOrWhiteSpace(authorPrefix) ? message : $"{authorPrefix}: {message}" ;
    }
}
