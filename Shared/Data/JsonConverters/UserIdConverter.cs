using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Data.TypedIds;

namespace Shared.Data.JsonConverters;

public class UserIdConverter : JsonConverter<UserId>
{
    public override UserId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var id = reader.GetString();
        return UserId.Parse(id);
    }

    public override void Write(Utf8JsonWriter writer, UserId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value.ToString());
    }
}
