using BlazorBootstrap;
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

    public bool Joined { get; private set; }
    public bool MicrophoneEnabled { get; private set; }
    public bool CameraEnabled { get; private set; }
    public bool ScreenShareEnabled { get; private set; }

    public IEnumerable<CallParticipantModel> Participants => _participants.Values;
    public IEnumerable<MediaDeviceModel> Microphones => _microphones;
    public IEnumerable<MediaDeviceModel> Cameras => _cameras;

    private readonly Dictionary<string, CallParticipantModel> _participants = [];
    private readonly List<MediaDeviceModel> _microphones = [];
    private readonly List<MediaDeviceModel> _cameras = [];

    public event Action? OnConnected;
    public event Func<DisconnectReason, Task>? OnDisconnectedAsync;
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

    public async Task JoinAsync(ChannelId channelId)
    {
        if (Joined) return;

        var token = await _liveKitService.CreateRoomTokenAsync(channelId);
        if (token is null)
        {
            OnError?.Invoke("Error while creating room access token");
            return;
        }

        var success = await _jsModule.TryInvokeAsync<bool>("Start", token.Token);
        if (!success)
        {
            OnError?.Invoke("Unable to join room");
            return;
        }

        Joined = true;
        OnConnected?.Invoke();
    }

    public async Task LeaveAsync()
    {
        await _jsModule.TryInvokeVoidAsync("Stop");
        Joined = false;
        _participants.Clear();
    }

    public async Task ChangeVolumeAsync(CallParticipantModel participant, int volume, AudioSource audioSource = AudioSource.Microphone)
    {
        await _jsModule.TryInvokeVoidAsync("ChangeVolume", participant.Username, volume);
    }

    public async Task SwapCameraAsync()
    {
        await _jsModule.TryInvokeVoidAsync("SwapCamera");
    }

    public async Task ShowVideoAsync(CallParticipantModel participant, VideoSource videoSource)
    {
        await _jsModule.TryInvokeVoidAsync("ShowVideo", participant.Username, videoSource);
    }

    public async Task MinimizeVideoAsync()
    {
        await _jsModule.TryInvokeVoidAsync("MinimizeVideo");
    }

    public Task SelectMicrophoneAsync(MediaDeviceModel microphone)
    {
        _logger.LogInformation("Selected microphone {Microphone}", microphone.Label);
        return Task.CompletedTask;
    }

    public Task SelectCameraAsync(MediaDeviceModel camera)
    {
        _logger.LogInformation("Selected camera {Camera}", camera.Label);
        return Task.CompletedTask;
    }

    public async Task ToggleMicrophoneAsync()
    {
        MicrophoneEnabled = !MicrophoneEnabled;
        await _jsModule.TryInvokeVoidAsync("SetMicrophoneEnabled", MicrophoneEnabled);
    }

    public async Task ToggleCameraAsync()
    {
        CameraEnabled = !ScreenShareEnabled;
        await _jsModule.TryInvokeVoidAsync("SetCameraEnabled", CameraEnabled);
    }

    public async Task ToggleScreenShareAsync()
    {
        ScreenShareEnabled = !ScreenShareEnabled;
        await _jsModule.TryInvokeVoidAsync("SetScreenShareEnabled", ScreenShareEnabled);
    }

    [JSInvokable]
    public void MicrophonesUpdatedCallback(MediaDeviceModel[] microphones)
    {
        _microphones.Clear();
        _microphones.AddRange(microphones);
    }

    [JSInvokable]
    public void CamerasUpdatedCallback(MediaDeviceModel[] cameras)
    {
        _cameras.Clear();
        _cameras.AddRange(cameras);
    }

    [JSInvokable]
    public void DisconnectedCallback(DisconnectReason reason)
    {
        Joined = false;
        _participants.Clear();

        if (reason is DisconnectReason.ClientInitiated) return;

        var toast = new ToastMessage(ToastType.Warning, "Disconnected", reason.ToString());
        _toastService.Notify(toast);
    }

    [JSInvokable]
    public void ErrorCallback(string error)
    {
        _toastService.Notify(new ToastMessage(ToastType.Warning, error));
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

    public async ValueTask DisposeAsync()
    {
        await _jsModule.TryInvokeVoidAsync("Stop");
        await _jsModule.TryDisposeAsync();

        _dotNetObjectReference.Dispose();
    }
}
