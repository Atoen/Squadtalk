using System.Text.Json.Nodes;

namespace Squadtalk.Data.LiveKit.Events;

public record RoomFinishedEvent : LiveKitEvent
{
    public RoomFinishedEvent(string eventName, string id, DateTimeOffset createdAt, JsonObject data)
        : base(eventName, id, createdAt) { }

}