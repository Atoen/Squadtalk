using BlazorBootstrap;
using FluentResults;
using Microsoft.JSInterop;
using Shared;
using Shared.Data.TypedIds;
using Shared.Models;
using Shared.Services;
using Squadtalk.Client.Extensions;

namespace Squadtalk.Client.Services;

public sealed class NewVoiceChatService : INewVoiceChatService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ToastService _toastService;
    private readonly ILiveKitService _liveKitService;
    private readonly ILogger<NewVoiceChatService> _logger;

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<NewVoiceChatService> _dotNetObjectReference;

    public NewVoiceChatService(
        IJSRuntime jsRuntime,
        ToastService toastService,
        ILiveKitService liveKitService,
        ILogger<NewVoiceChatService> logger)
    {
        _jsRuntime = jsRuntime;
        _toastService = toastService;
        _liveKitService = liveKitService;
        _logger = logger;

        _dotNetObjectReference = DotNetObjectReference.Create(this);
    }

    public bool JoinedRoom { get; private set; }

    public bool MicrophoneEnabled { get; private set; } = true;

    public bool CameraEnabled { get; private set; }

    public bool ScreenShareEnabled { get; private set; }

    public bool MicrophoneAvailable => _microphones.Count > 0;

    public bool CameraAvailable => _cameras.Count > 0;

    public IEnumerable<CallParticipantModel> Participants => _participants.Values;
    public IEnumerable<MediaDeviceModel> Microphones => _microphones;
    public IEnumerable<MediaDeviceModel> Cameras => _cameras;

    private readonly Dictionary<string, CallParticipantModel> _participants = [];
    private readonly List<MediaDeviceModel> _microphones = [];
    private readonly List<MediaDeviceModel> _cameras = [];

    public event Action? OnConnected;
    public event Action<DisconnectReason>? OnDisconnected;
    public event Action<MediaDeviceModel[]>? OnMicrophoneListUpdated;
    public event Action<MediaDeviceModel[]>? OnCameraListUpdated;
    public event Action<string>? OnError;
    public event Action<CallParticipantModel>? OnParticipantConnected;
    public event Action<CallParticipantModel>? OnDisplayParticipant;
    public event Action<CallParticipantModel>? OnParticipantDisconnected;

    public async Task InitializeAsync()
    {
        _jsModule ??= await _jsRuntime.ImportAndInitModuleAsync(JsModule.WebRTC, _dotNetObjectReference,
            "wss://192.168.1.134:1230/jajo");
    }

    public async Task JoinCallAsync(ChannelId channelId)
    {
        if (JoinedRoom) return;

        var token = await _liveKitService.CreateRoomTokenAsync(channelId);
        if (token is null)
        {
            OnError?.Invoke("Error while creating room access token");
            return;
        }

        var result = await _jsModule.TryInvokeAsync2<bool>("Start", token.Token);
        if (result.IsFailed)
        {
            DisplayErrors(result, "Unable to connect to room");
            return;
        }

        var joined = result.Value;
        if (!joined)
        {
            var toast = new ToastMessage(ToastType.Danger, "Unable to connect to room");
            _toastService.Notify(toast);
            return;
        }

        JoinedRoom = true;
        OnConnected?.Invoke();
    }

    public async Task LeaveCallAsync()
    {
        var result = await _jsModule.TryInvokeVoidAsync2("Stop");
        if (result.IsFailed)
        {
            DisplayErrors(result, "Error while disconnecting from room");
        }

        JoinedRoom = false;
        _participants.Clear();
    }

    public async Task ChangeVolumeAsync(CallParticipantModel participant, int volume, AudioSource audioSource = AudioSource.Microphone)
    {
        var result = await _jsModule.TryInvokeVoidAsync2("ChangeVolume", participant.Username, volume);
        if (result.IsFailed)
        {
            DisplayErrors(result, "Error while changing volume");
        }
    }

    public async Task SwapCameraAsync()
    {
        var result = await _jsModule.TryInvokeVoidAsync2("SwapCamera");
        if (result.IsFailed)
        {
            DisplayErrors(result, "Error while swapping camera");
        }
    }

    public async Task ShowVideoAsync(CallParticipantModel participant, VideoSource videoSource)
    {
        var result = await _jsModule.TryInvokeVoidAsync2("ShowVideo", participant.Username, videoSource);
        if (result.IsFailed)
        {
            DisplayErrors(result, "Error while showing video");
        }
    }

    public async Task MinimizeVideoAsync()
    {
        var result = await _jsModule.TryInvokeVoidAsync2("MinimizeVideo");
        if (result.IsFailed)
        {
            DisplayErrors(result, "Error while minimizing video");
        }
    }

    public async Task SelectMicrophoneAsync(MediaDeviceModel microphone)
    {
        var result = await _jsModule.TryInvokeVoidAsync2("ChangeDevice", InputDevice.Microphone, microphone.Id);
        if (result.IsFailed)
        {
            DisplayErrors(result, "Error while selecting microphone");
        }
    }

    public async Task SelectCameraAsync(MediaDeviceModel camera)
    {
        var result = await _jsModule.TryInvokeVoidAsync2("ChangeDevice", InputDevice.Camera, camera.Id);
        if (result.IsFailed)
        {
            DisplayErrors(result, "Error while selecting camera");
        }
    }

    public async Task ToggleMicrophoneAsync()
    {
        MicrophoneEnabled = !MicrophoneEnabled;
        var result = await _jsModule.TryInvokeVoidAsync2("SetMicrophoneEnabled", MicrophoneEnabled);
        if (result.IsFailed)
        {
            DisplayErrors(result, "Error while toggling microphone");
        }
    }

    public async Task ToggleCameraAsync()
    {
        CameraEnabled = !CameraEnabled;
        var result = await _jsModule.TryInvokeVoidAsync2("SetCameraEnabled", CameraEnabled);
        if (result.IsFailed)
        {
            DisplayErrors(result, "Error while toggling camera");
        }
    }

    public async Task ToggleScreenShareAsync()
    {
        ScreenShareEnabled = !ScreenShareEnabled;
        var result = await _jsModule.TryInvokeVoidAsync2("SetScreenShareEnabled", ScreenShareEnabled);
        if (result.IsFailed)
        {
            DisplayErrors(result, "Error while toggling screen share");
        }
    }

    [JSInvokable]
    public void MicrophonesUpdatedCallback(MediaDeviceModel[] microphones)
    {
        _microphones.Clear();
        _microphones.AddRange(microphones);
        OnMicrophoneListUpdated?.Invoke(microphones);
    }

    [JSInvokable]
    public void CamerasUpdatedCallback(MediaDeviceModel[] cameras)
    {
        _cameras.Clear();
        _cameras.AddRange(cameras);
        OnCameraListUpdated?.Invoke(cameras);
    }

    [JSInvokable]
    public void DisconnectedCallback(DisconnectReason reason)
    {
        JoinedRoom = false;
        _participants.Clear();

        OnDisconnected?.Invoke(reason);
        if (reason is DisconnectReason.ClientInitiated) return;

        var toast = new ToastMessage(ToastType.Warning, "Disconnected", reason.ToString());
        _toastService.Notify(toast);
    }

    [JSInvokable]
    public void ErrorCallback(string error)
    {
        _toastService.Notify(new ToastMessage(ToastType.Warning, error));
        OnError?.Invoke(error);
    }

    [JSInvokable]
    public void DisplayParticipantCallback(CallParticipantModel participant)
    {
        _participants[participant.Sid] = participant;
        OnDisplayParticipant?.Invoke(participant);
    }

    [JSInvokable]
    public void ParticipantConnectedCallback(CallParticipantModel participant)
    {
        _participants[participant.Sid] = participant;
        OnParticipantConnected?.Invoke(participant);
    }

    [JSInvokable]
    public void ParticipantDisconnectedCallback(CallParticipantModel participant)
    {
        _participants.Remove(participant.Sid);
        OnParticipantDisconnected?.Invoke(participant);
    }

    private void DisplayErrors(
        ResultBase result,
        string message,
        ToastType toastType = ToastType.Danger,
        bool autoHide = false)
    {
        var errors = string.Join(", ", result.Errors);
        var toast = new ToastMessage
        {
            Type = toastType,
            AutoHide = autoHide,
            Message = $"{message}: {errors}"
        };
        _toastService.Notify(toast);
    }

    public async ValueTask DisposeAsync()
    {
        await _jsModule.TryInvokeVoidAsync("Stop");
        await _jsModule.TryDisposeAsync();

        _dotNetObjectReference.Dispose();
    }
}
