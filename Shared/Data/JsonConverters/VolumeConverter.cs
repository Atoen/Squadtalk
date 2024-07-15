using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Services;

namespace Shared.Data.JsonConverters;

public class VolumeConverter : JsonConverter<Volume>
{
    public override Volume Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetInt32();
        return new Volume(value);
    }

    public override void Write(Utf8JsonWriter writer, Volume value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}