using Squadtalk.Shared;

namespace Squadtalk.Client.Models;

public class EmbedModel
{
    public EmbedType Type { get; set; }
    public Dictionary<string, string> Data { get; set; } = new();

    public string this[string key]
    {
        get => Data[key];
        set => Data[key] = value;
    }
}