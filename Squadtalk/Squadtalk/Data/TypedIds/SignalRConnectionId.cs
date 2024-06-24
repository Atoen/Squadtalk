using System.ComponentModel;
using Shared.Data.TypedIds;
using Shared.Data.TypedIds.TypeConverters;

namespace Squadtalk.Data.TypedIds;

[TypeConverter(typeof(StringIdConverter<SignalRConnectionId>))]
public record SignalRConnectionId(string Value) : StringIdRecord(Value), IStringIdRecord<SignalRConnectionId>
{
    public static explicit operator SignalRConnectionId(string id) => new(id);

    public override string ToString() => Value;

    public static SignalRConnectionId Create(string value) => new(value);
}