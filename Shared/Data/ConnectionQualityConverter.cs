using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Models;

namespace Shared.Data;

public class ConnectionQualityConverter : JsonConverter<ConnectionQuality>
{
    public override ConnectionQuality Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "excellent" => ConnectionQuality.Excellent,
            "good" => ConnectionQuality.Good,
            "poor" => ConnectionQuality.Poor,
            "lost" => ConnectionQuality.Lost,
            "unknown" => ConnectionQuality.Unknown,
            _ => throw new JsonException($"Unexpected value '{value}' for ConnectionQuality.")
        };
    }

    public override void Write(Utf8JsonWriter writer, ConnectionQuality value, JsonSerializerOptions options)
    {
        var stringValue = value switch
        {
            ConnectionQuality.Excellent => "excellent",
            ConnectionQuality.Good => "good",
            ConnectionQuality.Poor => "poor",
            ConnectionQuality.Lost => "lost",
            ConnectionQuality.Unknown => "unknown",
            _ => throw new JsonException($"Unexpected value '{value}' for ConnectionQuality.")
        };

        writer.WriteStringValue(stringValue);
    }
}