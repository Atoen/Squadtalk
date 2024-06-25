using System.ComponentModel;
using Shared.Data.TypedIds;

namespace Squadtalk.Data.TypedIds;

[TypeConverter(typeof(StringIdConverter<TusFileId>))]
public record TusFileId(string Value): StringIdRecord(Value), IStringIdRecord<TusFileId>
{
    public static explicit operator TusFileId(string id) => new(id);

    public override string ToString() => Value;

    public static TusFileId From(string value) => new(value);
    
    public static TusFileId New(StringIdValueFormat format = StringIdValueFormat.GuidN) =>
        new(GetFormattedValue(format));
}
