using MessagePack;
using Shared.Data;

namespace Shared.DTOs;

[MessagePackObject]
public class ChannelDto
{
    [Key(0)] public ChannelId Id { get; set; } = default!;

    [Key(1)] public List<UserDto> Participants { get; set; } = default!;

    [Key(2)] public MessageDto? LastMessage { get; set; }
}