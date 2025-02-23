using Shared.Data.TypedIds;
using Shared.Models;
using Shared.Reactive;

namespace Shared.Services;

public interface IRtcConnectionService
{
    event Action<ChatModel>? IncomingCall;

    bool ConnectedToCall { get; }

    ChatModel? CallChat { get; }

    CallParticipantModel LocalParticipant { get; }

    IObservableCollection<CallParticipantModel> CallParticipants { get; }

    Task StartCallAsync(ChatModel chat);

    Task AcceptCallAsync(ChatModel chat);

    Task DeclineCallAsync(ChatModel chat);

    Task LeaveCallAsync();

    CallParticipantModel? GetParticipantById(UserId userId);

    Task<bool> GroupHasActiveCallAsync(ChatModel chat);

    Task ChangeUserVolumeAsync(CallParticipantModel participant, Volume volume, AudioSource audioSource = AudioSource.Microphone);

    Task<Volume> GetSavedUserVolumeAsync(CallParticipantModel participant);
}

public interface IRtcMediaControlService
{
    bool MicrophoneAvailable { get; }
    bool CameraAvailable { get; }

    bool MicrophoneEnabled { get; }
    bool CameraEnabled { get; }
    bool ScreenShareEnabled { get; }

    CallParticipantModel LocalParticipant { get; }

    IObservableCollection<MicrophoneModel> Microphones { get; }
    IObservableCollection<CameraModel> Cameras { get; }

    MicrophoneModel? SelectedMicrophone { get; }
    CameraModel? SelectedCamera { get; }

    Task SelectMicrophoneAsync(MicrophoneModel microphone);
    Task SelectCameraAsync(CameraModel camera);

    Task ToggleMicrophoneAsync();
    Task ToggleCameraAsync();
    Task ToggleScreenShareAsync();
}
