using System.Text.Json.Serialization;
using Shared.Data.JsonConverters;
using Shared.Data.TypedIds;
using Shared.Services;

namespace Shared.Models;

public class CallParticipantModelOld
{
    public string Username { get; set; } = default!;

    public UserId Id { get; set; }
    public string Sid { get; set; } = default!;

    public bool MicrophoneOn { get; set; }
    public bool CameraOn { get; set; }
    public bool ScreenShareOn { get; set; }

    public Volume Volume { get; set; }

    public ConnectionQuality ConnectionQuality { get; set; }

    public bool IsRemote { get; set; }

    public bool IsSpeaking { get; set; }
}

[JsonConverter(typeof(ConnectionQualityConverter))]
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
