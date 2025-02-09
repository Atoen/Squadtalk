namespace Shared.Data.TypedIds;

public record SignalRConnectionId(string Value) : StringIdRecord(Value), IStringIdRecord<SignalRConnectionId>
{
    public static explicit operator SignalRConnectionId(string value) => new(value);

    public override string ToString() => Value;

    public static SignalRConnectionId From(string value) => new(value);

    public static SignalRConnectionId New(StringIdValueFormat format = StringIdValueFormat.GuidN) =>
        new(GetFormattedValue(format));
}
