using System.Text.Json.Nodes;

namespace Squadtalk.Data.LiveKit.Events;

public record IngressEndedEvent : LiveKitEvent
{
    public IngressEndedEvent(string eventName, string id, DateTimeOffset createdAt, JsonObject data)
        : base(eventName, id, createdAt) { }

}