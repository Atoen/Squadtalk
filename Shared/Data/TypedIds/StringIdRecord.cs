using MessagePack;
using Shared.Extensions;

namespace Shared.Data.TypedIds;

[MessagePackObject]
public abstract record StringIdRecord
{
    protected StringIdRecord(string value)
    {
        Value = !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException("Value must be non-empty", nameof(value));
    }
    
    [Key(0)]
    public string Value { get; }
    
    public static implicit operator string(StringIdRecord id) => id.Value;
    
    protected static string GetFormattedValue(StringIdValueFormat format)
    {
        var guid = Guid.NewGuid();
        return format switch
        {
            StringIdValueFormat.GuidN => guid.ToString("N"),
            StringIdValueFormat.GuidD => guid.ToString("D"),
            StringIdValueFormat.GuidB => guid.ToString("B"),
            StringIdValueFormat.GuidP => guid.ToString("P"),
            StringIdValueFormat.GuidX => guid.ToString("X"),
            StringIdValueFormat.Base64 => guid.ToString().ToBase64(),
            StringIdValueFormat.UrlFriendly => guid.ToString().ToBase64(urlEncode: true),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }
}
