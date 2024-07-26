namespace Shared.Models;

public class ParticipantState
{
    public bool MicrophoneOn { get; set; }
    public bool CameraOn { get; set; }
    public bool ScreenShareOn { get; set; }

    public ConnectionQuality ConnectionQuality { get; set; }
}
