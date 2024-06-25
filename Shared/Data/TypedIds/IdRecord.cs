using System.Text.Json.Serialization;
using MessagePack;

namespace Shared.Data.TypedIds;

[MessagePackObject]
public abstract record IdRecord
{
    [JsonConstructor]
    protected IdRecord(string value)
    {
        Value = !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException("Value must be non-empty", nameof(value));
    }

    [Key(0)]
    public string Value { get; }
    
    public static implicit operator string(IdRecord id) => id.Value;
}

public record CallId(string Value) : IdRecord(Value)
{
    public static explicit operator CallId(string id) => new(id);
    public static CallId New => new(Guid.NewGuid().ToString("N"));
}

public record CallOfferId(string Value) : IdRecord(Value)
{
    public static explicit operator CallOfferId(string id) => new(id);
    public static CallOfferId New => new(Guid.NewGuid().ToString("N"));
}