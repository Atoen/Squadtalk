using MessagePack;
using Shared.Data;
using Shared.Data.TypedIds;

namespace Shared.DTOs.Chat;

[MessagePackObject(AllowPrivate = true)]
public class MessageDto : IChatMessage
{
    [Key(0)] public MessageId Id { get; set; }

    [Key(1)] public UserDto Author { get; set; } = default!;

    [Key(2)] public string Content { get; set; } = default!;

    [Key(3)] public GroupId GroupId { get; set; } = default!;

    [Key(4)] public DateTimeOffset Timestamp { get; set; }

    [Key(5)] public EmbedDto? Embed { get; set; }

    [IgnoreMember]
    IChatUser IChatMessage.Author => Author;

    [IgnoreMember]
    IMessageEmbed? IChatMessage.Embed => Embed;
}
