using System.Text.Json.Nodes;

namespace Squadtalk.Data.LiveKit.Events;

public record RoomStartedEvent : LiveKitEvent
{
    public RoomStartedEvent(string eventName, string id, DateTimeOffset createdAt, JsonObject data)
        : base(eventName, id, createdAt)
    {
    }
}