using System.Text.Json.Nodes;

namespace Squadtalk.Data.LiveKit.Events;

public record ParticipantLeftEvent : LiveKitEvent
{
    public ParticipantLeftEvent(string eventName, string id, DateTimeOffset createdAt, JsonObject data)
        : base(eventName, id, createdAt) { }

}