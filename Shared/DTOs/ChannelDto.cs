using MessagePack;

namespace Shared.DTOs;

[MessagePackObject]
public class ChannelDto
{
    [Key(0)]
    public string Id { get; set; } = default!;

    [Key(1)]
    public List<UserDto> Participants { get; set; } = default!;

    [Key(2)]
    public string? LastMessage { get; set; } = default!;
    
    [Key(3)]
    public DateTimeOffset LastMessageTimestamp { get; set; }
}