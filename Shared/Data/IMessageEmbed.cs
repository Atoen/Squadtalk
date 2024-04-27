using Shared.Enums;

namespace Shared.Data;

public interface IMessageEmbed
{
    EmbedType Type { get; }
    
    Dictionary<string, string> Data { get; }
}