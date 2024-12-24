using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Data.TypedIds;

namespace Shared.Data.JsonConverters;

public class FriendRequestIdConverter : JsonConverter<FriendRequestId>
{
    public override FriendRequestId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var id = reader.GetInt32();
        return new FriendRequestId(id);
    }

    public override void Write(Utf8JsonWriter writer, FriendRequestId value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}
