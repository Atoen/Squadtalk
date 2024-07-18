using System.Text.Json.Nodes;

namespace Squadtalk.Data.LiveKit.Events;

public record TrackPublishedEvent : LiveKitEvent
{
    public TrackPublishedEvent(string eventName, string eventId, DateTimeOffset createdAt, JsonObject data)
        : base(eventName, eventId, createdAt) { }

}