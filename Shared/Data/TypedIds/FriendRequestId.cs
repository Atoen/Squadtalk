using System.Text.Json.Serialization;
using MessagePack;
using Shared.Data.JsonConverters;

namespace Shared.Data.TypedIds;

[MessagePackObject]
[JsonConverter(typeof(FriendRequestIdConverter))]
public readonly record struct FriendRequestId([property: Key(0)] int Value);
