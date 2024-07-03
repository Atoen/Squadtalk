using Shared.Data.TypedIds;
using Shared.Models;

namespace Shared.Services;

public interface INewVoiceChatService
{
    bool Joined { get; }

    bool MicrophoneEnabled { get; }

    bool CameraEnabled { get; }

    bool ScreenShareEnabled { get; }

    IEnumerable<CallParticipantModel> Participants { get; }
    IEnumerable<MediaDeviceModel> Microphones { get; }
    IEnumerable<MediaDeviceModel> Cameras { get; }

    event Action? OnConnected;
    event Func<DisconnectReason, Task>? OnDisconnectedAsync;
    event Action<MediaDeviceModel[]>? OnMicrophoneListUpdated;
    event Action<MediaDeviceModel[]>? OnCameraListUpdated;
    event Action<string>? OnError;
    event Action<CallParticipantModel>? OnParticipantConnected;
    event Action<CallParticipantModel>? OnDisplayParticipant;
    event Action<CallParticipantModel>? OnParticipantDisconnected;

    Task InitializeAsync();

    Task JoinAsync(ChannelId channelId);

    Task LeaveAsync();

    Task ChangeVolumeAsync(CallParticipantModel participant, int volume, AudioSource audioSource = AudioSource.Microphone);

    Task SwapCameraAsync();

    Task ShowVideoAsync(CallParticipantModel participant, VideoSource videoSource);

    Task MinimizeVideoAsync();

    Task SelectMicrophoneAsync(MediaDeviceModel microphone);

    Task SelectCameraAsync(MediaDeviceModel camera);

    Task ToggleMicrophoneAsync();

    Task ToggleCameraAsync();

    Task ToggleScreenShareAsync();
}

public enum AudioSource
{
    Microphone,
    ScreenShareAudio
}

public enum VideoSource
{
    Camera,
    ScreenShare
}