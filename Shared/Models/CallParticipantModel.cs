using System.Text.Json.Serialization;
using Shared.Data.JsonConverters;
using Shared.Data.TypedIds;

namespace Shared.Models;

public class CallParticipantModel
{
    public string Username { get; set; } = default!;

    [JsonConverter(typeof(UserIdConverter))]
    public UserId Id { get; set; }
    public string Sid { get; set; } = default!;

    public bool MicrophoneOn { get; set; }
    public bool CameraOn { get; set; }
    public bool ScreenShareOn { get; set; }

    public int Volume { get; set; }
    public long Bitrate { get; set; }

    [JsonConverter(typeof(ConnectionQualityConverter))]
    public ConnectionQuality ConnectionQuality { get; set; }

    public bool Remote { get; set; }

    public bool IsSpeaking { get; set; }
}

public enum ConnectionQuality
{
    Excellent,
    Good,
    Poor,
    Lost,
    Unknown
}

public enum VideoStream
{
    Camera = 1,
    ScreenShare
}

public enum DisconnectReason
{
    Unknown,
    ClientInitiated,
    DuplicateIdentity,
    ServerShutdown,
    ParticipantRemoved,
    RoomDeleted,
    StateMismatch,
    JoinFailure,
    Migration,
    SignalClose
}
