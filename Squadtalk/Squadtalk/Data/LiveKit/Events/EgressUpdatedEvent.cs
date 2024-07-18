using System.Text.Json.Nodes;

namespace Squadtalk.Data.LiveKit.Events;

public record EgressUpdatedEvent : LiveKitEvent
{
    public EgressUpdatedEvent(string eventName, string eventId, DateTimeOffset createdAt, JsonObject data)
        : base(eventName, eventId, createdAt) { }

}
