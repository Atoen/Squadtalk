using MessagePack;
using Shared.Data;
using Shared.Enums;

namespace Shared.DTOs.Chat;

[MessagePackObject]
public class EmbedDto : IMessageEmbed
{
    [Key(0)] public EmbedType Type { get; set; }

    [Key(1)] public Dictionary<string, string> Data { get; set; } = default!;

    public string this[string key] => Data.GetValueOrDefault(key, "-");
}