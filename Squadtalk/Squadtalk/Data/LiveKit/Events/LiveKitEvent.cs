using System.Text.Json.Nodes;

namespace Squadtalk.Data.LiveKit.Events;

public abstract record LiveKitEvent(string EventName, string EventId, DateTimeOffset CreatedAt)
{
    public const string RoomStarted = "room_started";
    public const string RoomFinished = "room_finished";
    public const string ParticipantJoined = "participant_joined";
    public const string ParticipantLeft = "participant_left";
    public const string TrackPublished = "track_published";
    public const string TrackUnpublished = "track_unpublished";
    public const string EgressStarted = "egress_started";
    public const string EgressUpdated = "egress_updated";
    public const string EgressEnded = "egress_ended";
    public const string IngressStarted = "ingress_started";
    public const string IngressEnded = "ingress_ended";

    public static LiveKitEvent Create(JsonObject jsonObject)
    {
        var eventName = jsonObject["event"]!.ToString();
        var eventId = jsonObject["id"]!.ToString();

        var createdAt = jsonObject["createdAt"]!.ToString();
        var timestamp = long.Parse(createdAt);
        var createdAtTimestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).ToLocalTime();

        return eventName switch
        {
            RoomStarted => new RoomStartedEvent(eventName, eventId, createdAtTimestamp, jsonObject),
            RoomFinished => new RoomFinishedEvent(eventName, eventId, createdAtTimestamp, jsonObject),
            ParticipantJoined => new ParticipantJoinedEvent(eventName, eventId, createdAtTimestamp, jsonObject),
            ParticipantLeft => new ParticipantLeftEvent(eventName, eventId, createdAtTimestamp, jsonObject),
            TrackPublished => new TrackPublishedEvent(eventName, eventId, createdAtTimestamp, jsonObject),
            TrackUnpublished => new TrackUnpublishedEvent(eventName, eventId, createdAtTimestamp, jsonObject),
            EgressStarted => new EgressStartedEvent(eventName, eventId, createdAtTimestamp, jsonObject),
            EgressUpdated => new EgressUpdatedEvent(eventName, eventId, createdAtTimestamp, jsonObject),
            EgressEnded => new EgressEndedEvent(eventName, eventId, createdAtTimestamp, jsonObject),
            IngressStarted => new IngressStartedEvent(eventName, eventId, createdAtTimestamp, jsonObject),
            IngressEnded => new IngressEndedEvent(eventName, eventId, createdAtTimestamp, jsonObject),
            _ => throw new ArgumentException("Invalid event name")
        };
    }
}
