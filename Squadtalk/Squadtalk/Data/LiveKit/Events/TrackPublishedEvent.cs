using System.Text.Json.Nodes;

namespace Squadtalk.Data.LiveKit.Events;

public record TrackPublishedEvent : LiveKitEvent
{
    public TrackPublishedEvent(string eventName, string id, DateTimeOffset createdAt, JsonObject data)
        : base(eventName, id, createdAt) { }

}