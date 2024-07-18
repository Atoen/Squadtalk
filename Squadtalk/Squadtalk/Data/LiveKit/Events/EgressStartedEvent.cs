using System.Text.Json.Nodes;

namespace Squadtalk.Data.LiveKit.Events;

public record EgressStartedEvent : LiveKitEvent
{
    public EgressStartedEvent(string eventName, string eventId, DateTimeOffset createdAt, JsonObject data)
        : base(eventName, eventId, createdAt) { }

}
