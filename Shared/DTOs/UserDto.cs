using MessagePack;

namespace Shared.DTOs;

[MessagePackObject]
public class UserDto
{
    [Key(0)] public string Username { get; set; } = default!;

    [Key(1)] public string Id { get; init; } = default!;
}