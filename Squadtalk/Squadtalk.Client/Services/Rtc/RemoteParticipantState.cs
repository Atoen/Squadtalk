using Shared.Data.TypedIds;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Client.Services.Rtc;

public class RemoteParticipantState
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
