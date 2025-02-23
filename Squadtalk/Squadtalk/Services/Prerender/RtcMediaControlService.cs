using Shared.Models;
using Shared.Reactive;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal sealed class RtcMediaControlService(IRtcConnectionService connectionService) : IRtcMediaControlService
{
    public bool MicrophoneAvailable => false;
    public bool CameraAvailable => false;

    public bool MicrophoneEnabled => false;
    public bool CameraEnabled => false;
    public bool ScreenShareEnabled => false;
    public CallParticipantModel LocalParticipant => connectionService.LocalParticipant;

    public IObservableCollection<MicrophoneModel> Microphones => ObservableCollection<MicrophoneModel>.Empty;
    public IObservableCollection<CameraModel> Cameras => ObservableCollection<CameraModel>.Empty;

    public MicrophoneModel? SelectedMicrophone => null;
    public CameraModel? SelectedCamera => null;

    public Task SelectMicrophoneAsync(MicrophoneModel microphone) => Task.CompletedTask;
    public Task SelectCameraAsync(CameraModel camera) => Task.CompletedTask;
    public Task ToggleMicrophoneAsync() => Task.CompletedTask;
    public Task ToggleCameraAsync() => Task.CompletedTask;
    public Task ToggleScreenShareAsync() => Task.CompletedTask;
}
