using MessagePack;

namespace Shared.DTOs;

[MessagePackObject]
public class MessageDto
{
    [Key(0)]
    public UserDto Author { get; set; } = default!;
    
    [Key(1)]
    public string Content { get; set; } = default!;

    [Key(2)]
    public string ChannelId { get; set; } = default!;

    [Key(3)]
    public DateTimeOffset Timestamp { get; set; }

    [Key(4)]
    public EmbedDto? Embed { get; set; }
}