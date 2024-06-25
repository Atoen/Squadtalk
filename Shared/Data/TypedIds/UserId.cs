using System.ComponentModel;
using MessagePack;

namespace Shared.Data.TypedIds;

[MessagePackObject]
[TypeConverter(typeof(GuidIdConverter<UserId>))]
public readonly record struct UserId([property: Key(0)] Guid Value) : IGuidIdRecord<UserId>
{
    public static UserId New => new(Guid.NewGuid());

    public static UserId Empty => default;

    public static UserId From(Guid value) => new(value);
    
    public override string ToString() => Value.ToString();

    public static UserId Parse(ReadOnlySpan<char> span) => new(Guid.Parse(span));

    public static bool TryParse(ReadOnlySpan<char> span, out UserId idRecord)
    {
        var success = Guid.TryParse(span, out var guid);
        idRecord = new UserId(guid);

        return success;
    }
}
