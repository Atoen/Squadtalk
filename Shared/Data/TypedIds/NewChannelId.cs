using System.ComponentModel;
using MessagePack;
using Shared.Data.TypedIds.TypeConverters;

namespace Shared.Data.TypedIds;

[MessagePackObject]
[TypeConverter(typeof(GuidIdConverter<NewChannelId>))]
public readonly record struct NewChannelId([property: Key(0)] Guid Value) : IGuidIdRecord<NewChannelId>
{
    public static NewChannelId New => new(Guid.NewGuid());

    public static NewChannelId Empty => default;

    public override string ToString() => Value.ToString();
    
    public static NewChannelId Create(Guid value) => new(value);
}
