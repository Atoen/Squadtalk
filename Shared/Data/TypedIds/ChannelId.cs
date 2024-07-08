using System.ComponentModel;
using System.Text.Json.Serialization;
using Shared.Data.JsonConverters;

namespace Shared.Data.TypedIds;

[JsonConverter(typeof(ChannelIdConverter))]
[TypeConverter(typeof(StringIdConverter<ChannelId>))]
public record ChannelId(string Value) : StringIdRecord(Value), IStringIdRecord<ChannelId>
{
    public static explicit operator ChannelId(string value) => new(value);
    
    public override string ToString() => Value;

    public static ChannelId From(string value) => new(value);

    public static ChannelId New(StringIdValueFormat format = StringIdValueFormat.GuidN) =>
        new(GetFormattedValue(format));
}
