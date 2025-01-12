using MessagePack;
using Shared.Extensions;

namespace Shared.Data;

[MessagePackObject]
public readonly record struct TextChannelCursor([property: Key(0)] long Value)
{
    public override string ToString() => Value.ToString().ToBase64(urlEncode: true);

    public static TextChannelCursor New => new(DateTimeOffset.UtcNow.UtcTicks);
}