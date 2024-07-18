using System.Text.Json;
using System.Text.Json.Nodes;
using Squadtalk.Data.LiveKit.DTOs;

namespace Squadtalk.Data.LiveKit.Events;

public record RoomFinishedEvent : LiveKitEvent
{
    public RoomDto Room { get; }

    public RoomFinishedEvent(string eventName, string eventId, DateTimeOffset createdAt, JsonObject data)
        : base(eventName, eventId, createdAt)
    {
        Room = data["room"].Deserialize<RoomDto>()!;
    }
}
