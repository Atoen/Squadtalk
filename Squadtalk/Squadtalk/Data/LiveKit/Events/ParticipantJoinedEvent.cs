using System.Text.Json;
using System.Text.Json.Nodes;
using Squadtalk.Data.LiveKit.DTOs;

namespace Squadtalk.Data.LiveKit.Events;

public record ParticipantJoinedEvent : LiveKitEvent
{
    public RoomDto Room { get; }
    public ParticipantDto Participant { get; }

    public ParticipantJoinedEvent(string eventName, string eventId, DateTimeOffset createdAt, JsonObject data)
        : base(eventName, eventId, createdAt)
    {
        Room = data["room"].Deserialize<RoomDto>()!;
        Participant = data["participant"].Deserialize<ParticipantDto>()!;
    }
}
