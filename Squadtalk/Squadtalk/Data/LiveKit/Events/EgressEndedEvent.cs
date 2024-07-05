using System.Text.Json.Nodes;

namespace Squadtalk.Data.LiveKit.Events;

public record EgressEndedEvent : LiveKitEvent
{
    public EgressEndedEvent(string eventName, string id, DateTimeOffset createdAt, JsonObject data)
        : base(eventName, id, createdAt) { }

}