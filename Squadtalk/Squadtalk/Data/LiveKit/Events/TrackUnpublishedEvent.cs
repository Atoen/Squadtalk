using System.Text.Json.Nodes;

namespace Squadtalk.Data.LiveKit.Events;

public record TrackUnpublishedEvent : LiveKitEvent
{
    public TrackUnpublishedEvent(string eventName, string id, DateTimeOffset createdAt, JsonObject data)
        : base(eventName, id, createdAt) { }

}