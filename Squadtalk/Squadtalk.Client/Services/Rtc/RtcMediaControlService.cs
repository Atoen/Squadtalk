using Shared.Models;
using Shared.Reactive;
using Shared.Services;

namespace Squadtalk.Client.Services.Rtc;

internal sealed class RtcMediaControlService : IRtcMediaControlService
{
    private readonly IRtcConnectionService _rtcConnectionService;
    private readonly VoiceChatInterop _voiceChatInterop;

    private readonly ObservableList<MicrophoneModel> _microphones =
    [
        new("Microphone 1", "mic1"),
        new("Microphone 2", "mic2")
    ];

    private readonly ObservableList<CameraModel> _cameras =
    [
        new("Camera 1", "cam1"),
        new("Camera 2", "cam2")
    ];

    public bool MicrophoneAvailable => _microphones.Count != 0;
    public bool CameraAvailable => _cameras.Count != 0;

    public bool MicrophoneEnabled => _rtcConnectionService.LocalParticipant.MicrophoneEnabled;
    public bool CameraEnabled { get; private set; }
    public bool ScreenShareEnabled { get; private set; }

    public CallParticipantModel LocalParticipant => _rtcConnectionService.LocalParticipant;

    public IObservableCollection<MicrophoneModel> Microphones => _microphones;
    public IObservableCollection<CameraModel> Cameras => _cameras;

    public MicrophoneModel? SelectedMicrophone { get; private set; }
    public CameraModel? SelectedCamera { get; private set; }

    public RtcMediaControlService(
        IRtcConnectionService rtcConnectionService,
        VoiceChatInterop voiceChatInterop)
    {
        _rtcConnectionService = rtcConnectionService;
        _voiceChatInterop = voiceChatInterop;
        _voiceChatInterop.SetMediaControlCallback(this);
    }

    public Task SelectMicrophoneAsync(MicrophoneModel microphone)
    {
        if (_microphones.Contains(microphone))
        {
            SelectedMicrophone = microphone;
        }
        return Task.CompletedTask;
    }

    public Task SelectCameraAsync(CameraModel camera)
    {
        if (_cameras.Contains(camera))
        {
            SelectedCamera = camera;
        }
        return Task.CompletedTask;
    }

    public async Task ToggleMicrophoneAsync()
    {
        if (!MicrophoneEnabled && SelectedMicrophone is null)
        {
            SelectedMicrophone = _microphones.FirstOrDefault();
        }

        var afterToggle = await _voiceChatInterop.ToggleMicrophoneAsync();

        _rtcConnectionService.LocalParticipant.MicrophoneEnabled = afterToggle;
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

    internal void MicrophoneListUpdated(MediaDeviceInfo[] microphonesInfo)
    {
        var newMics = microphonesInfo.Select(x => new MicrophoneModel(x.Label, x.Id));
        _microphones.Refresh(newMics);
    }

    internal void CameraListUpdated(MediaDeviceInfo[] camerasInfo)
    {
        var newCameras = camerasInfo.Select(x => new CameraModel(x.Label, x.Id));
        _cameras.Refresh(newCameras);
    }
}
