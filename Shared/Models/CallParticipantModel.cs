using System.Text.Json.Serialization;
using Shared.Data;

namespace Shared.Models;

public class CallParticipantModel
{
    public string Username { get; set; } = default!;

    public long Bitrate { get; set; }

    public bool MicrophoneOn { get; set; }
    public bool CameraOn { get; set; }
    public bool ScreenShareOn { get; set; }

    public int Volume { get; set; }

    [JsonConverter(typeof(ConnectionQualityConverter))]
    public ConnectionQuality ConnectionQuality { get; set; } = default!;

    public bool Remote { get; set; }

    public bool IsSpeaking { get; set; }

    public string Sid { get; set; } = default!;
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