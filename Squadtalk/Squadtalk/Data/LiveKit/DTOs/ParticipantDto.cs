using System.Text.Json.Serialization;

namespace Squadtalk.Data.LiveKit.DTOs;

public class ParticipantDto
{
    [JsonPropertyName("sid")] public string Sid { get; set; } = default!;

    [JsonPropertyName("identity")] public string Identity { get; set; } = default!;
}
