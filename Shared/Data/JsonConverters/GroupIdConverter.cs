using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Data.TypedIds;

namespace Shared.Data.JsonConverters;

public class GroupIdConverter : JsonConverter<GroupId>
{
    public override GroupId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var id = reader.GetString();
        return id is null ? null : GroupId.From(id);
    }

    public override void Write(Utf8JsonWriter writer, GroupId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
