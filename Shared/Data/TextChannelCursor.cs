using MessagePack;
using Shared.Data.TypedIds;
using Shared.Extensions;

namespace Shared.Data;

[MessagePackObject]
public readonly record struct TextChannelCursor([property: Key(0)] MessageId Value);