using Shared.Models;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal class RtcMediaControlService : IRtcMediaControlService
{
    public bool MicrophoneAvailable => false;
    public bool CameraAvailable => false;

    public bool MicrophoneEnabled => false;
    public bool CameraEnabled => false;
    public bool ScreenShareEnabled => false;

    public IEnumerable<MediaDeviceModel> Microphones => [];
    public IEnumerable<MediaDeviceModel> Cameras => [];

    public MediaDeviceModel? SelectedMicrophone => null;
    public MediaDeviceModel? SelectedCamera => null;

    public Task SelectMicrophoneAsync(MediaDeviceModel microphone) => Task.CompletedTask;
    public Task SelectCameraAsync(MediaDeviceModel camera) => Task.CompletedTask;
    public Task ToggleMicrophoneAsync() => Task.CompletedTask;
    public Task ToggleCameraAsync() => Task.CompletedTask;
    public Task ToggleScreenShareAsync() => Task.CompletedTask;
}
