using Shared.Models;

namespace Shared.Services;

public interface IRtcConnectionService
{
    bool ConnectedToCall { get; }

    Task StartCallAsync(ChatModel chat);

    Task AcceptCallAsync(ChatModel chat);

    Task DeclineCallAsync(ChatModel chat);

    Task LeaveCallAsync(ChatModel chat);

    Task<bool> GroupHasActiveCallAsync(ChatModel chat);
}

public interface IRtcMediaControlService
{
    bool MicrophoneAvailable { get; }
    bool CameraAvailable { get; }

    bool MicrophoneEnabled { get; }
    bool CameraEnabled { get; }
    bool ScreenShareEnabled { get; }

    IEnumerable<MediaDeviceModel> Microphones { get; }
    IEnumerable<MediaDeviceModel> Cameras { get; }

    MediaDeviceModel? SelectedMicrophone { get; }
    MediaDeviceModel? SelectedCamera { get; }

    Task SelectMicrophoneAsync(MediaDeviceModel microphone);
    Task SelectCameraAsync(MediaDeviceModel camera);

    Task ToggleMicrophoneAsync();
    Task ToggleCameraAsync();
    Task ToggleScreenShareAsync();
}

public interface IRtcCallParticipantManager
{
    IEnumerable<CallParticipantModelOld> ActiveCallParticipants { get; }

    event Action? LocalParticipantStateUpdated;

    Task ChangeVolumeAsync(CallParticipantModelOld participant, Volume volume, AudioSource audioSource = AudioSource.Microphone);
    Task<Volume> GetUserVolumeAsync(CallParticipantModelOld participantModelOld);
    Task MaximizeVideoAsync(CallParticipantModelOld participant, VideoSource videoSource);
    Task MinimizeVideoAsync();
}
