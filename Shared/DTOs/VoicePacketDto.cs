using MessagePack;

namespace Shared.DTOs;

[MessagePackObject]
public class VoicePacketDto
{
    [Key(0)] string Sender { get; init; } = default!;
    
    [Key(1)] byte[] Data { get; init; } = default!;
}