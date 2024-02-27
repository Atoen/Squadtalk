using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Squadtalk.Shared;

public class EmbedDto
{
    public EmbedType Type { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public string DataJson { get; set; } = default!;

    [NotMapped]
    public Dictionary<string, string> Data
    {
        get => JsonConvert.DeserializeObject<Dictionary<string, string>>(DataJson)!;
        set => DataJson = JsonConvert.SerializeObject(value);
    }

    public string this[string key]
    {
        get => Data[key];
        set => Data[key] = value;
    }
}

public enum EmbedType
{
    File,
    Image,
    Gif
}