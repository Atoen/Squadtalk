using System.Text.Json.Nodes;

namespace Squadtalk.Data.LiveKit.Events;

public record TrackUnpublishedEvent : LiveKitEvent
{
    public TrackUnpublishedEvent(string eventName, string eventId, DateTimeOffset createdAt, JsonObject data)
        : base(eventName, eventId, createdAt) { }

}