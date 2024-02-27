using Microsoft.EntityFrameworkCore;
using Shared.Enums;

namespace Squadtalk.Data;

[Owned]
public class Embed
{
    public EmbedType Type { get; set; }
    public Dictionary<string, string> Data { get; set; } = new();

    public string this[string key]
    {
        get => Data[key];
        set => Data[key] = value;
    }
}