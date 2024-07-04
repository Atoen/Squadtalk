using System.ComponentModel;
using MessagePack;

namespace Shared.Data.TypedIds;

[MessagePackObject]
[TypeConverter(typeof(GuidIdConverter<CallId>))]
public readonly record struct CallId([property: Key(0)] Guid Value) : IGuidIdRecord<CallId>
{
    public static CallId New => new(Guid.NewGuid());

    public static CallId Empty => default;

    public static CallId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();

    public static CallId Parse(ReadOnlySpan<char> span) => new(Guid.Parse(span));

    public static bool TryParse(ReadOnlySpan<char> span, out CallId idRecord)
    {
        var success = Guid.TryParse(span, out var guid);
        idRecord = new CallId(guid);

        return success;
    }
}