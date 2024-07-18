using System.Text.Json.Serialization;
using Shared.Data.TypedIds;

namespace Squadtalk.Data.LiveKit.DTOs;

public class ParticipantDto
{
    [JsonPropertyName("sid")] public string Sid { get; set; } = default!;

    [JsonPropertyName("identity")] public UserId Id { get; set; } = default!;

    [JsonPropertyName("name")] public string Username { get; set; } = default!;
}
