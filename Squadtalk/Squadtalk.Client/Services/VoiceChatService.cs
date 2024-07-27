using System.Runtime.CompilerServices;
using System.Security.Claims;
using FluentResults;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Shared;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs;
using Shared.Extensions;
using Shared.Models;
using Shared.Services;
using Squadtalk.Client.Extensions;

namespace Squadtalk.Client.Services;

public sealed partial class VoiceChatService : IVoiceChatService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ICommunicationService _communicationService;
    private readonly IChatService _chatService;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly UserVolumeManager _volumeManager;
    private readonly ILogger<VoiceChatService> _logger;

    private readonly DotNetObjectReference<VoiceChatService> _dotNetObjectReference;
    private IJSObjectReference? _jsModule;

    public bool ConnectedToVoiceCall { get; private set; }

    public bool ActiveCallOnCurrentChannel => CurrentChannel?.State.HasActiveCall ?? false;

    public bool ConnectedToVoiceCallOnCurrentChannel => ConnectedToVoiceCall && CurrentChannel == CallChannel;

    public ChannelModel? CallChannel { get; private set; }

    public ChannelModel? CurrentChannel => _chatService.CurrentChannel;

    public bool MicrophoneEnabled { get; private set; }

    public bool CameraEnabled { get; private set; }

    public bool ScreenShareEnabled { get; private set; }

    public bool MicrophoneAvailable => _microphones.Count > 0;

    public bool CameraAvailable => _cameras.Count > 0;

    public ConnectionQuality ConnectionQuality { get; private set; } = ConnectionQuality.Unknown;

    public IEnumerable<CallParticipantModel> ActiveCallParticipants => _participants.Values;

    public IEnumerable<MediaDeviceModel> Microphones => _microphones;

    public IEnumerable<MediaDeviceModel> Cameras => _cameras;

    private readonly Dictionary<UserId, CallParticipantModel> _participants = [];
    private readonly List<MediaDeviceModel> _microphones = [];
    private readonly List<MediaDeviceModel> _cameras = [];

    private readonly PeriodicTimer _pingTimer = new(TimeSpan.FromMilliseconds(500));

    public event Action? OnMicrophoneListUpdated;
    public event Action? OnCameraListUpdated;
    public event ErrorNotificationHandler? OnError;
    public event Action<DisconnectReason>? OnDisconnected;
    public event Action? OnCurrentChannelCallChanged;
    public event Func<ChannelModel, Task>? OnCallIncoming;
    public event Action<ChannelModel>? OnCallEnded;
    public event Action<ChannelModel>? OnParticipantsUpdated;
    public event Action? LocalParticipantStateUpdated;

    public VoiceChatService(
        IJSRuntime jsRuntime,
        ICommunicationService communicationService,
        IChatService chatService,
        AuthenticationStateProvider authenticationStateProvider,
        UserVolumeManager volumeManager,
        ILogger<VoiceChatService> logger)
    {
        _jsRuntime = jsRuntime;
        _communicationService = communicationService;
        _chatService = chatService;
        _authenticationStateProvider = authenticationStateProvider;
        _volumeManager = volumeManager;
        _logger = logger;

        _dotNetObjectReference = DotNetObjectReference.Create(this);
        _communicationService.IncomingCall += IncomingCall;
        _communicationService.CallEnded += CallEnded;
        _communicationService.CallDeclined += CallDeclined;
        _communicationService.CallAccepted += CallAccepted;
        _communicationService.CallFailed += CallFailed;

        // _ = PingAsync();
    }

    private async Task PingAsync()
    {
        while (await _pingTimer.WaitForNextTickAsync())
        {
            var ping = await _communicationService.MeasureClientDelayAsync();
            _logger.LogInformation("Client ping: {Ping} ms", ping.Milliseconds);
        }
    }

    public async Task InitializeAsync()
    {
        if (_jsModule is null)
        {
            _jsModule = await _jsRuntime.ImportAndInitModuleAsync(JsModule.WebRTC, _dotNetObjectReference,
                "wss://192.168.1.134:1230/jajo");
        }
        else
        {
            await _jsModule.TryInvokeVoidAsync2("GetElements");
        }
    }

    public async Task StartCallAsync(ChannelId channelId)
    {
        var roomToken = await _communicationService.StartVoiceCallAsync(channelId);
        if (roomToken is null)
        {
            OnError?.Invoke("Error while initiating call", "Failed to create room token");
            _logger.LogError("Call offer id is null");
            return;
        }

        var currentChannel = _chatService.CurrentChannel;
        if (currentChannel?.Id != channelId)
        {
            OnError?.Invoke("Error while initiating call","Channel not found");
            _logger.LogError("Channel is null");
            return;
        }

        await JoinRoomAsync(roomToken, currentChannel);
    }

    public async Task AcceptCallAsync(ChannelId id)
    {
        var token = await _communicationService.AcceptCallAsync(id);
        if (token is null)
        {
            OnError?.Invoke("Error while joining the call", "Failed to create room token");
            _logger.LogInformation("Null token from accepting");
            return;
        }

        var channel = _chatService.GetChannel(id)!;
        await JoinRoomAsync(token, channel);
    }

    public Task DeclineCallAsync(ChannelId id)
    {
        return _communicationService.DeclineCallAsync(id);
    }

    public async Task LeaveCallAsync()
    {
        var result = await _jsModule.TryInvokeVoidAsync2("Stop");
        if (result.IsFailed)
        {
            NotifyOnError("Error while disconnecting from call", result);
        }

        ConnectedToVoiceCall = false;
        _participants.Clear();
    }

    public async Task ChangeVolumeAsync(CallParticipantModel participant, Volume volume, AudioSource audioSource = AudioSource.Microphone)
    {
        var result = await _jsModule.TryInvokeVoidAsync2("ChangeVolume", participant.Id, volume.Value);
        if (result.IsFailed)
        {
            NotifyOnError("Error while changing user volume", result);
            return;
        }

        await _volumeManager.SaveUserVolumeAsync(participant.Id, volume);
    }

    public async Task<Volume> GetUserVolumeAsync(CallParticipantModel participant)
    {
        return await _volumeManager.GetUserVolumeAsync(participant.Id);
    }

    public async Task SwapCameraAsync()
    {
        var result = await _jsModule.TryInvokeVoidAsync2("SwapCamera");
        NotifyIfFailed(result);
    }

    public async Task MaximizeVideoAsync(CallParticipantModel participant, VideoSource videoSource)
    {
        var result = await _jsModule.TryInvokeVoidAsync2("MaximizeVideo", participant.Id, videoSource);
        NotifyIfFailed(result);
    }

    public async Task MinimizeVideoAsync()
    {
        var result = await _jsModule.TryInvokeVoidAsync2("MinimizeVideo");
        NotifyIfFailed(result);
    }

    public async Task SelectMicrophoneAsync(MediaDeviceModel microphone)
    {
        var result = await _jsModule.TryInvokeVoidAsync2("ChangeDevice", InputDevice.Microphone, microphone.Id);
        NotifyIfFailed(result);
    }

    public async Task SelectCameraAsync(MediaDeviceModel camera)
    {
        var result = await _jsModule.TryInvokeVoidAsync2("ChangeDevice", InputDevice.Camera, camera.Id);
        NotifyIfFailed(result);
    }

    public async Task ToggleMicrophoneAsync()
    {
        var result = await _jsModule.TryInvokeAsync2<bool>("ToggleMicrophoneEnabled");
        if (result.IsFailed)
        {
            NotifyOnError("Error while toggling microphone", result);
            return;
        }

        var lastEnabled = MicrophoneEnabled;
        if (result.Value != lastEnabled)
        {
            MicrophoneEnabled = result.Value;
            LocalParticipantStateUpdated?.Invoke();
        }
    }

    public async Task ToggleCameraAsync()
    {
        var result = await _jsModule.TryInvokeAsync2<bool>("ToggleCameraEnabled");
        if (result.IsFailed)
        {
            NotifyOnError("Error while toggling camera", result);
            return;
        }

        var lastEnabled = CameraEnabled;
        if (result.Value != lastEnabled)
        {
            CameraEnabled = result.Value;
            LocalParticipantStateUpdated?.Invoke();
        }
    }

    public async Task ToggleScreenShareAsync()
    {
        var result = await _jsModule.TryInvokeAsync2<bool>("ToggleScreenShareEnabled");
        if (result.IsFailed)
        {
            NotifyOnError("Error while toggling screenShare", result);
            return;
        }

        var lastEnabled = ScreenShareEnabled;
        if (result.Value != lastEnabled)
        {
            ScreenShareEnabled = result.Value;
            LocalParticipantStateUpdated?.Invoke();
        }
    }

    public async Task<bool> CheckIfChannelHasActiveCallAsync(ChannelModel channel)
    {
        var hasCall = await _communicationService.ChannelHasActiveCall(channel.Id);

        channel.State.HasActiveCall = hasCall;
        if (hasCall)
        {
            OnCurrentChannelCallChanged?.Invoke();
        }

        return hasCall;
    }

    private async Task JoinRoomAsync(RoomTokenDto token, ChannelModel channel)
    {
        var result = await _jsModule.TryInvokeAsync2<bool>("Start", token.Token);
        if (result.IsFailed)
        {
            NotifyOnError("Error while joining room", result);
            return;
        }

        var joined = result.Value;
        if (!joined)
        {
            // OnError?.Invoke("Error while joining room", "Unable to connect to the server");
            return;
        }

        channel.State.HasActiveCall = true;
        ConnectedToVoiceCall = true;
        CallChannel = channel;

        // MicrophoneEnabled = true;
        // CameraEnabled = false;
        // ScreenShareEnabled = false;

        OnCurrentChannelCallChanged?.Invoke();
    }

    private async Task IncomingCall(ChannelId channelId, UserId initiatorId)
    {
        if (_chatService.GetChannel(channelId) is not { } channel)
        {
            _logger.LogError("Call on null channel");
            return;
        }

        if (channel.State.HasActiveCall) return;

        channel.State.HasActiveCall = true;

        var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var id = UserId.Parse(authenticationState.User.GetRequiredClaimValue(ClaimTypes.NameIdentifier));

        if (initiatorId == id) return;

        _logger.LogInformation("Incoming call from: {Caller}", channelId.Value);

        OnCurrentChannelCallChanged?.Invoke();
        await OnCallIncoming.TryInvoke(channel);
    }

    private Task CallAccepted(ChannelId channelId, IChatUser accepting)
    {
        _logger.LogInformation("User {User} accepted call", accepting.Username);

        if (!_participants.ContainsKey(accepting.Id))
        {
            _participants[accepting.Id] = new CallParticipantModel
            {
                Username = $"(Accepting) {accepting.Username}",
                ConnectionQuality = ConnectionQuality.Unknown
            };

            OnParticipantsUpdated?.Invoke(_chatService.GetRequiredChannel(channelId));
        }

        return Task.CompletedTask;
    }

    private Task CallDeclined(IChatUser declining, ChannelId channelId)
    {
        _logger.LogInformation("Call {CallId} declined by {User}", channelId, declining.Username);
        return Task.CompletedTask;
    }

    private Task CallEnded(ChannelId channelId)
    {
        _logger.LogInformation("Call {Id} ended", channelId);

        var channel = _chatService.GetRequiredChannel(channelId);
        channel.State.HasActiveCall = false;

        OnCallEnded?.Invoke(channel);
        OnCurrentChannelCallChanged?.Invoke();

        return Task.CompletedTask;
    }

    private Task CallFailed(string reason)
    {
        OnError?.Invoke("Error when creating call", reason);
        return Task.CompletedTask;
    }

    private bool MediaDevicesMatches(List<MediaDeviceModel> firstList, List<MediaDeviceModel> secondList)
    {
        if (firstList.Count != secondList.Count) return false;
        for (var i = 0; i < firstList.Count; i++)
        {
            if (firstList[i].Id != secondList[i].Id) return false;
        }

        return true;
    }

    private void NotifyIfFailed(ResultBase result, [CallerMemberName] string callerName = "")
    {
        if (result.IsFailed)
        {
            NotifyOnError(callerName, result);
        }
    }

    private void NotifyOnError(string title, ResultBase result)
    {
        var errors = string.Join(", ", result.Errors);
        OnError?.Invoke(title, errors);
    }

    public async ValueTask DisposeAsync()
    {
        await _jsModule.TryInvokeVoidAsync("Stop");
        await _jsModule.TryDisposeAsync();

        _dotNetObjectReference.Dispose();
    }
}
