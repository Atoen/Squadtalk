using System.Text.Json;
using System.Text.Json.Nodes;
using Squadtalk.Data.LiveKit.DTOs;

namespace Squadtalk.Data.LiveKit.Events;

public record RoomFinishedEvent : LiveKitEvent
{
    public RoomDto Room { get; }

    public RoomFinishedEvent(string eventName, string id, DateTimeOffset createdAt, JsonObject data)
        : base(eventName, id, createdAt)
    {
        Room = data["room"].Deserialize<RoomDto>()!;
    }
}
