using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Data.TypedIds;

namespace Shared.Data.JsonConverters;

public class ChannelIdConverter : JsonConverter<ChannelId>
{
    public override ChannelId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var id = reader.GetString();
        return id is null ? null : ChannelId.From(id);
    }

    public override void Write(Utf8JsonWriter writer, ChannelId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
