using System.Text.Json.Nodes;

namespace Squadtalk.Data.LiveKit.Events;

public abstract record LiveKitEvent(string EventName, string Id, DateTimeOffset CreatedAt)
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
        var id = jsonObject["id"]!.ToString();

        var createdAt = jsonObject["createdAt"]!.ToString();
        var timestamp = long.Parse(createdAt);
        var createdAtTimestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).ToLocalTime();

        return eventName switch
        {
            RoomStarted => new RoomStartedEvent(eventName, id, createdAtTimestamp, jsonObject),
            RoomFinished => new RoomFinishedEvent(eventName, id, createdAtTimestamp, jsonObject),
            ParticipantJoined => new ParticipantJoinedEvent(eventName, id, createdAtTimestamp, jsonObject),
            ParticipantLeft => new ParticipantLeftEvent(eventName, id, createdAtTimestamp, jsonObject),
            TrackPublished => new TrackPublishedEvent(eventName, id, createdAtTimestamp, jsonObject),
            TrackUnpublished => new TrackUnpublishedEvent(eventName, id, createdAtTimestamp, jsonObject),
            EgressStarted => new EgressStartedEvent(eventName, id, createdAtTimestamp, jsonObject),
            EgressUpdated => new EgressUpdatedEvent(eventName, id, createdAtTimestamp, jsonObject),
            EgressEnded => new EgressEndedEvent(eventName, id, createdAtTimestamp, jsonObject),
            IngressStarted => new IngressStartedEvent(eventName, id, createdAtTimestamp, jsonObject),
            IngressEnded => new IngressEndedEvent(eventName, id, createdAtTimestamp, jsonObject),
            _ => throw new ArgumentException("Invalid event name")
        };
    }
}
