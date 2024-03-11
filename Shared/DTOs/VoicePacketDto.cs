using MessagePack;
using Shared.Data;

namespace Shared.DTOs;

[MessagePackObject]
public record VoicePacketDto([property: Key(0)] UserId Id, [property: Key(1)] byte[] Data);