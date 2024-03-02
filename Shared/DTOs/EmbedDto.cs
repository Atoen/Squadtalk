using MessagePack;
using Shared.Enums;

namespace Shared.DTOs;

[MessagePackObject]
public class EmbedDto
{
    [Key(0)] public EmbedType Type { get; set; }

    [Key(1)] public Dictionary<string, string> Data { get; set; } = default!;

    [IgnoreMember] public string this[string key] => Data[key];
}