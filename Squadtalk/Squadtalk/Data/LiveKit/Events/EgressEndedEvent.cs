using System.Text.Json.Nodes;

namespace Squadtalk.Data.LiveKit.Events;

public record EgressEndedEvent : LiveKitEvent
{
    public EgressEndedEvent(string eventName, string eventId, DateTimeOffset createdAt, JsonObject data)
        : base(eventName, eventId, createdAt) { }

}
