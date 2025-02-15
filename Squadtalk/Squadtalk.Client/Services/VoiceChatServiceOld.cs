using System.Runtime.CompilerServices;
using FluentResults;
using Microsoft.JSInterop;
using Shared;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs;
using Shared.Extensions;
using Shared.Models;
using Shared.Services;
using Squadtalk.Client.Extensions;
using Squadtalk.Client.Services.SignalR;
using Squadtalk.Client.Services.SignalR.Interfaces;

namespace Squadtalk.Client.Services;

internal sealed partial class VoiceChatServiceOld : IVoiceChatServiceOld, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ISignalrRTCService _signalrRTCService;
    private readonly IChatManager _chatManager;
    private readonly UserVolumeManager _volumeManager;
    private readonly IUserAuthenticationService _userAuthenticationService;
    private readonly ILogger<VoiceChatServiceOld> _logger;

    private readonly DotNetObjectReference<VoiceChatServiceOld> _dotNetObjectReference;
    private IJSObjectReference? _jsModule;

    public bool ConnectedToVoiceCall { get; private set; }
    public bool ActiveCallOnCurrentChannel => CurrentChannel?.State.HasActiveCall ?? false;
    public bool ConnectedToVoiceCallOnCurrentChannel => ConnectedToVoiceCall && CurrentChannel == CallChannel;

    public ChatModel? CallChannel { get; private set; }
    public ChatModel? CurrentChannel => _chatManager.CurrentChat;

    public bool MicrophoneEnabled { get; private set; }
    public bool CameraEnabled { get; private set; }
    public bool ScreenShareEnabled { get; private set; }

    public bool MicrophoneAvailable => _microphones.Count > 0;
    public bool CameraAvailable => _cameras.Count > 0;

    public ConnectionQuality ConnectionQuality { get; private set; } = ConnectionQuality.Unknown;

    public IEnumerable<CallParticipantModelOld> ActiveCallParticipants => _participants.Values;

    public IEnumerable<MediaDeviceModel> Microphones => _microphones;
    public IEnumerable<MediaDeviceModel> Cameras => _cameras;

    private readonly Dictionary<UserId, CallParticipantModelOld> _participants = [];
    private readonly List<MediaDeviceModel> _microphones = [];
    private readonly List<MediaDeviceModel> _cameras = [];

    public event Action? MicrophoneListUpdated;
    public event Action? CameraListUpdated;
    public event ErrorNotificationHandler? Error;
    public event Action<DisconnectReason>? Disconnected;
    public event Action? CurrentChannelCallChanged;
    public event Func<ChatModel, Task>? CallIncoming;
    public event Action<ChatModel>? CallEnded;
    public event Action<ChatModel>? ParticipantListUpdated;
    public event Action<UserId, ChatModel>? ParticipantUpdated;
    public event Action? LocalParticipantStateUpdated;

    public VoiceChatServiceOld(
        IJSRuntime jsRuntime,
        SignalrService signalrRTCService,
        IChatManager chatManager,
        UserVolumeManager volumeManager,
        IUserAuthenticationService userAuthenticationService,
        ILogger<VoiceChatServiceOld> logger)
    {
        _jsRuntime = jsRuntime;
        _signalrRTCService = signalrRTCService;
        _chatManager = chatManager;
        _volumeManager = volumeManager;
        _userAuthenticationService = userAuthenticationService;
        _logger = logger;

        _dotNetObjectReference = DotNetObjectReference.Create(this);

        _signalrRTCService.IncomingCall += OnIncomingCall;
        _signalrRTCService.CallEnded += OnCallEnded;
        _signalrRTCService.CallDeclined += OnCallDeclined;
        _signalrRTCService.CallAccepted += OnCallAccepted;
        _signalrRTCService.CallFailed += OnCallFailed;
    }

    public async Task InitializeAsync()
    {
        _jsModule ??= await _jsRuntime.ImportAndInitModuleAsync(JsModule.WebRTC, _dotNetObjectReference,
            "wss://192.168.1.134:1230/jajo");
    }

    public async Task StartCallAsync(GroupId groupId)
    {
        var roomToken = await _signalrRTCService.StartVoiceCallAsync(groupId);
        if (roomToken.IsError)
        {
            Error?.Invoke("Error while initiating call", "Failed to create room token");
            _logger.LogError("Call offer id is null");
            return;
        }

        var currentChannel = _chatManager.CurrentChat;
        if (currentChannel?.Id != groupId)
        {
            Error?.Invoke("Error while initiating call","Channel not found");
            _logger.LogError("Channel is null");
            return;
        }

        await JoinRoomAsync(roomToken.Value, currentChannel);
    }

    public async Task AcceptCallAsync(GroupId id)
    {
        var token = await _signalrRTCService.AcceptCallAsync(id);
        if (token.IsError)
        {
            Error?.Invoke("Error while joining the call", "Failed to create room token");
            _logger.LogInformation("Null token from accepting");
            return;
        }

        var channel = _chatManager.GetChannel(id)!;
        await JoinRoomAsync(token.Value, channel);
    }

    public Task DeclineCallAsync(GroupId id)
    {
        return _signalrRTCService.DeclineCallAsync(id);
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

    public async Task ChangeVolumeAsync(CallParticipantModelOld participant, Volume volume, AudioSource audioSource = AudioSource.Microphone)
    {
        var result = await _jsModule.TryInvokeVoidAsync2("ChangeVolume", participant.Id, volume.Value);
        if (result.IsFailed)
        {
            NotifyOnError("Error while changing user volume", result);
            return;
        }

        await _volumeManager.SaveUserVolumeAsync(participant.Id, volume);
    }

    public async Task<Volume> GetUserVolumeAsync(CallParticipantModelOld participant)
    {
        return await _volumeManager.GetUserVolumeAsync(participant.Id);
    }

    public async Task SwapCameraAsync()
    {
        var result = await _jsModule.TryInvokeVoidAsync2("SwapCamera");
        NotifyIfFailed(result);
    }

    public async Task MaximizeVideoAsync(CallParticipantModelOld participant, VideoSource videoSource)
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

    public async Task<bool> CheckIfChannelHasActiveCallAsync(ChatModel chat)
    {
        var hasCall = await _signalrRTCService.GroupHasActiveCall(chat.Id);

        if (hasCall.IsError)
        {
            return false;
        }

        chat.State.HasActiveCall = hasCall.Value;
        if (hasCall.Value)
        {
            CurrentChannelCallChanged?.Invoke();
        }

        return hasCall.Value;
    }

    private async Task JoinRoomAsync(RoomTokenDto token, ChatModel chat)
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
            return;
        }

        chat.State.HasActiveCall = true;
        ConnectedToVoiceCall = true;
        CallChannel = chat;

        CurrentChannelCallChanged?.Invoke();
    }

    private Task OnIncomingCall(GroupId groupId, UserId initiatorId)
    {
        if (_chatManager.GetChannel(groupId) is not { } channel || channel.State.HasActiveCall)
        {
            return Task.CompletedTask;
        }

        channel.State.HasActiveCall = true;
        if (initiatorId == _userAuthenticationService.UserId)
        {
            return Task.CompletedTask;
        }

        _logger.LogInformation("Incoming call from: {Caller}", groupId.Value);

        CurrentChannelCallChanged?.Invoke();
        return CallIncoming.TryInvoke(channel);
    }

    private Task OnCallAccepted(GroupId groupId, IChatUser accepting)
    {
        _logger.LogInformation("User {User} accepted call", accepting.Username);

        if (!_participants.ContainsKey(accepting.Id))
        {
            _participants[accepting.Id] = new CallParticipantModelOld
            {
                Username = accepting.Username,
                ConnectionQuality = ConnectionQuality.Unknown
            };

            ParticipantListUpdated?.Invoke(_chatManager.GetRequiredChannel(groupId));
        }

        return Task.CompletedTask;
    }

    private Task OnCallDeclined(IChatUser declining, GroupId groupId)
    {
        _logger.LogInformation("Call {CallId} declined by {User}", groupId, declining.Username);
        return Task.CompletedTask;
    }

    private Task OnCallEnded(GroupId groupId)
    {
        _logger.LogInformation("Call {Id} ended", groupId);

        var channel = _chatManager.GetRequiredChannel(groupId);
        channel.State.HasActiveCall = false;

        CallEnded?.Invoke(channel);
        CurrentChannelCallChanged?.Invoke();

        return Task.CompletedTask;
    }

    private Task OnCallFailed(string reason)
    {
        Error?.Invoke("Error when creating call", reason);
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
        Error?.Invoke(title, errors);
    }

    public async ValueTask DisposeAsync()
    {
        await _jsModule.TryInvokeVoidAsync("Stop");
        await _jsModule.TryDisposeAsync();

        _dotNetObjectReference.Dispose();
    }
}
