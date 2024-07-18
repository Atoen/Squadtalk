using System.Text.Json;
using System.Text.Json.Nodes;
using Squadtalk.Data.LiveKit.DTOs;

namespace Squadtalk.Data.LiveKit.Events;

public record RoomStartedEvent : LiveKitEvent
{
    public RoomDto Room { get; }

    public RoomStartedEvent(string eventName, string eventId, DateTimeOffset createdAt, JsonObject data)
        : base(eventName, eventId, createdAt)
    {
        Room = data["room"].Deserialize<RoomDto>()!;
    }
}
