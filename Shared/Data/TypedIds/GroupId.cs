using System.ComponentModel;
using System.Text.Json.Serialization;
using Shared.Data.JsonConverters;

namespace Shared.Data.TypedIds;

[JsonConverter(typeof(GroupIdConverter))]
[TypeConverter(typeof(StringIdConverter<GroupId>))]
public record GroupId(string Value) : StringIdRecord(Value), IStringIdRecord<GroupId>
{
    public static explicit operator GroupId(string value) => new(value);

    public override string ToString() => Value;

    public static GroupId From(string value) => new(value);

    public static GroupId New(StringIdValueFormat format = StringIdValueFormat.GuidN) =>
        new(GetFormattedValue(format));
}
