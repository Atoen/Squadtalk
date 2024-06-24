using System.ComponentModel;
using MessagePack;
using Shared.Data.TypedIds.TypeConverters;

namespace Shared.Data.TypedIds;

[MessagePackObject]
[TypeConverter(typeof(GuidIdConverter<NewUserId>))]
public readonly record struct NewUserId([property: Key(0)] Guid Value) : IGuidIdRecord<NewUserId>
{
    public static NewUserId New => new(Guid.NewGuid());

    public static NewUserId Empty => default;

    public override string ToString() => Value.ToString();
    
    public static NewUserId Create(Guid value) => new(value);
}