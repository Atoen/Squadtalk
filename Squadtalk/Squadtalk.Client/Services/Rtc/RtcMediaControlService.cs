using Shared.Models;
using Shared.Services;

namespace Squadtalk.Client.Services.Rtc;

internal class RtcMediaControlService : IRtcMediaControlService
{
    private readonly List<MediaDeviceModel> _microphones =
    [
        new() { Label = "Microphone 1", Id = "mic1" },
        new() { Label = "Microphone 2", Id = "mic2" }
    ];

    private readonly List<MediaDeviceModel> _cameras =
    [
        new() { Label = "Camera 1", Id = "cam1" },
        new() { Label = "Camera 2", Id = "cam2" }
    ];

    public bool MicrophoneAvailable => _microphones.Count != 0;
    public bool CameraAvailable => _cameras.Count != 0;

    public bool MicrophoneEnabled { get; private set; }
    public bool CameraEnabled { get; private set; }
    public bool ScreenShareEnabled { get; private set; }

    public IEnumerable<MediaDeviceModel> Microphones => _microphones;
    public IEnumerable<MediaDeviceModel> Cameras => _cameras;

    public MediaDeviceModel? SelectedMicrophone { get; private set; }
    public MediaDeviceModel? SelectedCamera { get; private set; }

    public Task SelectMicrophoneAsync(MediaDeviceModel microphone)
    {
        if (_microphones.Contains(microphone))
        {
            SelectedMicrophone = microphone;
        }
        return Task.CompletedTask;
    }

    public Task SelectCameraAsync(MediaDeviceModel camera)
    {
        if (_cameras.Contains(camera))
        {
            SelectedCamera = camera;
        }
        return Task.CompletedTask;
    }

    public Task ToggleMicrophoneAsync()
    {
        if (!MicrophoneEnabled && SelectedMicrophone is null)
        {
            SelectedMicrophone = _microphones.FirstOrDefault();
        }

        MicrophoneEnabled = !MicrophoneEnabled;
        return Task.CompletedTask;
    }

    public Task ToggleCameraAsync()
    {
        if (!CameraEnabled && SelectedCamera is null)
        {
            SelectedCamera = _cameras.FirstOrDefault();
        }

        CameraEnabled = !CameraEnabled;
        return Task.CompletedTask;
    }

    public Task ToggleScreenShareAsync()
    {
        ScreenShareEnabled = !ScreenShareEnabled;
        return Task.CompletedTask;
    }
}
