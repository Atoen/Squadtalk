using MessagePack;

namespace Shared.DTOs;

[MessagePackObject]
public class RoomTokenDto
{
    [Key(0)]
    public string Token { get; set; } = default!;
}
