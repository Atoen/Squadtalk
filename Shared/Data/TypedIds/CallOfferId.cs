using System.ComponentModel;
using MessagePack;

namespace Shared.Data.TypedIds;

[MessagePackObject]
[TypeConverter(typeof(GuidIdConverter<CallOfferId>))]
public readonly record struct CallOfferId([property: Key(0)] Guid Value) : IGuidIdRecord<CallOfferId>
{
    public static CallOfferId New => new(Guid.NewGuid());

    public static CallOfferId Empty => default;

    public static CallOfferId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();

    public static CallOfferId Parse(ReadOnlySpan<char> span) => new(Guid.Parse(span));

    public static bool TryParse(ReadOnlySpan<char> span, out CallOfferId idRecord)
    {
        var success = Guid.TryParse(span, out var guid);
        idRecord = new CallOfferId(guid);

        return success;
    }
}