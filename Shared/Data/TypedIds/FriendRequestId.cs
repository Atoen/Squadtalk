using System.Text.Json.Serialization;
using Shared.Data.JsonConverters;

namespace Shared.Data.TypedIds;

[JsonConverter(typeof(FriendRequestIdConverter))]
public readonly record struct FriendRequestId(int Value);
