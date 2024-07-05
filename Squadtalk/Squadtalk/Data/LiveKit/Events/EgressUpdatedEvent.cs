using System.Text.Json.Nodes;

namespace Squadtalk.Data.LiveKit.Events;

public record EgressUpdatedEvent : LiveKitEvent
{
    public EgressUpdatedEvent(string eventName, string id, DateTimeOffset createdAt, JsonObject data)
        : base(eventName, id, createdAt) { }

}