using Microsoft.EntityFrameworkCore;
using Shared.Data;
using Shared.Enums;

namespace Squadtalk.Data.Entities;

[Owned]
public class Embed : IMessageEmbed
{
    public EmbedType Type { get; set; }
    public Dictionary<string, string> Data { get; set; } = new();

    public string this[string key]
    {
        get => Data.GetValueOrDefault(key, "-");
        set => Data[key] = value;
    }
}
