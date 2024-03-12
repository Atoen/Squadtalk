using MessagePack;

namespace Shared.Data;

[MessagePackObject]
public abstract record IdRecord(string Value)
{
    [Key(0)]
    public string Value { get; } =
        !string.IsNullOrWhiteSpace(Value)
            ? Value
            : throw new ArgumentException("Value must be non-empty", nameof(Value));
    
    public static implicit operator string(IdRecord id) => id.Value;
}

public record SignalrConnectionId(string Value) : IdRecord(Value)
{
    public static explicit operator SignalrConnectionId(string value) => new(value);
    public static SignalrConnectionId New => new(Guid.NewGuid().ToString("N"));
}

public record UserId(string Value) : IdRecord(Value)
{
    public static explicit operator UserId(string id) => new(id);
    public static UserId New => new(Guid.NewGuid().ToString("N"));
    public override string ToString() => Value;
}

public record ChannelId(string Value) : IdRecord(Value)
{
    public static explicit operator ChannelId(string id) => new(id);
    public static ChannelId New => new(Guid.NewGuid().ToString("N"));
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