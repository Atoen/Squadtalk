using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Squadtalk.Data.LiveKit.DTOs;

public class RoomDto
{
    [JsonPropertyName("name")] public string Id { get; set; } = default!;

    [DefaultValue(0)]
    [JsonPropertyName("participants")]
    public int Participants { get; set; }
}
