using System.ComponentModel;
using Shared.Data.TypedIds;
using Shared.Data.TypedIds.TypeConverters;

namespace Squadtalk.Data.TypedIds;

[TypeConverter(typeof(StringIdConverter<TusFileId>))]
public record TusFileId(string Value): StringIdRecord(Value), IStringIdRecord<TusFileId>
{
    public static explicit operator TusFileId(string id) => new(id);

    public override string ToString() => Value;

    public static TusFileId Create(string value) => new(value);
}
