using Shared.Enums;

namespace Shared.DTOs;

public class EmbedDto
{
    public EmbedType Type { get; set; }

    public Dictionary<string, string> Data { get; set; } = default!;
    
    public string this[string key] => Data[key];
}