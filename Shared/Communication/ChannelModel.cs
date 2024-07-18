using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;

namespace Shared.Communication;

public abstract class ChannelModel(ChannelId id)
{
    public const string CurrentUserAuthorPrefix = "You";

    public abstract string Name { get; }
    
    public string? LastMessage { get; private set; }
    public DateTimeOffset LastMessageTimeStamp { get; private set; }
    
    public ChannelId Id { get; } = id;
    
    public ChannelState State { get; } = new();
    
    public void SetLastMessage(IChatMessage message, bool byCurrentUser)
    {
        var contentToDisplay = message.Embed switch
        {
            { Type: EmbedType.File } => "Sent file",
            { Type: EmbedType.Image } => "Sent image",
            { Type: EmbedType.Video } => "Sent video",
            { Type: EmbedType.SystemMessage } => message.Embed.Data[EmbedData.SystemMessageData],
            _ => message.Content
        };

        var isSystemMessage = message.Embed?.Type == EmbedType.SystemMessage;

        var authorPrefix = this switch
        {
            _ when isSystemMessage => string.Empty,
            _ when byCurrentUser => CurrentUserAuthorPrefix,
            DirectMessageChannelModel => string.Empty,
            _ => message.Author.Username
        };

        SetLastMessage(contentToDisplay, message.Timestamp, authorPrefix);
    }

    public void SetLastMessage(string message, DateTimeOffset timestamp, string authorPrefix)
    {
        LastMessageTimeStamp = timestamp;
        LastMessage = string.IsNullOrWhiteSpace(authorPrefix) ? message : $"{authorPrefix}: {message}" ;
    }
}
