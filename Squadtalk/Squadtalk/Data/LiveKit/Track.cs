namespace Squadtalk.Data.LiveKit;

public record Track(
    string Sid,
    bool DisableDtx,
    TrackSource Source,
    string MimeType,
    string Mid,
    bool Stereo,
    bool DisableRed);

public enum TrackSource
{
    Microphone,
    Camera,
    ScreenShare,
    ScreenShareAudio
}