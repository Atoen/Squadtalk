using MessagePack;

namespace Shared.Data.TypedIds;

[MessagePackObject]
public readonly record struct MessageId([property: Key(0)] uint Value)
{
    public static bool operator >(MessageId left, MessageId right) => left.Value > right.Value;
    public static bool operator <(MessageId left, MessageId right) => left.Value < right.Value;
}
