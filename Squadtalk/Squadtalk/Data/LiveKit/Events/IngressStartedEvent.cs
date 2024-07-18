using System.Text.Json.Nodes;

namespace Squadtalk.Data.LiveKit.Events;

public record IngressStartedEvent : LiveKitEvent
{
    public IngressStartedEvent(string eventName, string eventId, DateTimeOffset createdAt, JsonObject data)
        : base(eventName, eventId, createdAt) { }

}
