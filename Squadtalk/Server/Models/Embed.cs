using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Squadtalk.Shared;

namespace Squadtalk.Server.Models;

[Owned]
public class Embed
{
    public EmbedType Type { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public string DataJson { get; set; } = default!;

    [NotMapped]
    public Dictionary<string, string> Data
    {
        get => JsonSerializer.Deserialize<Dictionary<string, string>>(DataJson)!;
        set => JsonSerializer.Serialize(value);
    }

    public EmbedDto ToDto()
    {
        return new EmbedDto
        {
            Type = Type,
            Data = Data
        };
    }
}