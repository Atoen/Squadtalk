using System.ComponentModel;
using Shared.Data.TypedIds;

namespace Squadtalk.Data.TypedIds;

[TypeConverter(typeof(StringIdConverter<SignalRConnectionId>))]
public record SignalRConnectionId(string Value) : StringIdRecord(Value), IStringIdRecord<SignalRConnectionId>
{
    public static explicit operator SignalRConnectionId(string id) => new(id);

    public override string ToString() => Value;

    public static SignalRConnectionId From(string value) => new(value);

    public static SignalRConnectionId New(StringIdValueFormat format = StringIdValueFormat.GuidN) =>
        new(GetFormattedValue(format));
}
