using MessagePack;

namespace Shared.DTOs;

[MessagePackObject]
public class VoiceCallDto
{
    [Key(0)] public UserDto Initiator { get; set; } = default!;
    
    [Key(1)] public IList<UserDto> Invited { get; set; } = default!;
    
    [Key(2)] public IList<string> ConnectedIds { get; set; } = default!;

    [Key(3)] public string Id { get; set; } = default!;
}