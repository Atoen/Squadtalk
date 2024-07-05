using System.Text.Json.Nodes;

namespace Squadtalk.Data.LiveKit.Events;

public record ParticipantJoinedEvent : LiveKitEvent
{
    public ParticipantJoinedEvent(string eventName, string id, DateTimeOffset createdAt, JsonObject data)
        : base(eventName, id, createdAt) { }

}