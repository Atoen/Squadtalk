using Shared.Models;

namespace Squadtalk.Client.Services.Rtc;

public sealed class LocalParticipantState
{
    public bool MicrophoneOn { get; set; }
    public bool CameraOn { get; set; }
    public bool ScreenShareOn { get; set; }

    public ConnectionQuality ConnectionQuality { get; set; }

    public bool IsSpeaking { get; set; }
}
