using MessagePack;

namespace Shared.DTOs;

[MessagePackObject]
public class VoicePacketDto
{
    [Key(0)] public string Sender { get; init; } = default!;
    
    [Key(1)] public byte[] Data { get; init; } = default!;
}