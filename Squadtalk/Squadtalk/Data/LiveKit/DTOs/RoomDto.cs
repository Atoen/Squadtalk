using System.ComponentModel;
using System.Text.Json.Serialization;
using Shared.Data.TypedIds;

namespace Squadtalk.Data.LiveKit.DTOs;

public class RoomDto
{
    [JsonPropertyName("name")]
    public GroupId GroupId { get; set; } = default!;

    [JsonPropertyName("sid")]
    public string CallId { get; set; } = default!;

    [DefaultValue(0)]
    [JsonPropertyName("participants")]
    public int Participants { get; set; }
}
