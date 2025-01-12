using MessagePack;
using Shared.Data;
using Shared.Data.TypedIds;

namespace Shared.DTOs.Chat;

[MessagePackObject(AllowPrivate = true)]
public class MessageDto : IChatMessage
{
    [Key(0)] public UserDto Author { get; set; } = default!;

    [Key(1)] public string Content { get; set; } = default!;

    [Key(2)] public ChannelId ChannelId { get; set; } = default!;

    [Key(3)] public DateTimeOffset Timestamp { get; set; }

    [Key(4)] public EmbedDto? Embed { get; set; }
    
    [IgnoreMember]
    IChatUser IChatMessage.Author => Author;
    
    [IgnoreMember]
    IMessageEmbed? IChatMessage.Embed => Embed;
}