using Shared.Extensions;

namespace Shared.Data;

public readonly record struct TextChannelCursor(long Value)
{
    public override string ToString() => Value.ToString().ToBase64(urlEncode: true);
}