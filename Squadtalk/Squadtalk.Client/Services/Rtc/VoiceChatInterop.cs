using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Implementation;
using Shared;
using Shared.Data.TypedIds;
using Shared.DTOs;
using Shared.Models;
using Shared.Services;
using Squadtalk.Client.Extensions;

namespace Squadtalk.Client.Services.Rtc;

internal sealed class VoiceChatInterop : IDisposable
{
    private const string RtcScheme = "wss";

    private readonly ILogger<VoiceChatInterop> _logger;
    private readonly IJSInProcessRuntime _jsRuntime;
    private readonly DotNetObjectReference<VoiceChatInterop> _dotNetObjectReference;

    private IJSInProcessObjectReference? _jsModule;
    private bool _importedModule;

    private RtcConnectionService? _connectionCallback;
    private RtcMediaControlService? _mediaControlCallback;

    public bool Initialized { get; private set; }

    public VoiceChatInterop(
        IJSRuntime jsRuntime,
        ILogger<VoiceChatInterop> logger)
    {
        _jsRuntime = (JSInProcessRuntime) jsRuntime;
        _logger = logger;

        _dotNetObjectReference = DotNetObjectReference.Create(this);
    }

    public void SetConnectionCallback(RtcConnectionService connectionService)
    {
        Debug.Assert(_connectionCallback is null);
        _connectionCallback = connectionService;
    }

    public void SetMediaControlCallback(RtcMediaControlService mediaControlService)
    {
        Debug.Assert(_mediaControlCallback is null);
        _mediaControlCallback = mediaControlService;
    }

    public async Task InitializeAsync(string rtcEndpoint)
    {
        if (!Uri.TryCreate(rtcEndpoint, UriKind.Absolute, out var uri) || uri.Scheme != RtcScheme)
        {
            _logger.LogError("RTC endpoint uri {Uri} is invalid", rtcEndpoint);
            return;
        }

        if (_importedModule) return;
        _importedModule = true;

        var module = await _jsRuntime.ImportAndInitModuleAsync(
            JsModule.WebRTC, _dotNetObjectReference, rtcEndpoint);

        _jsModule = (JSInProcessObjectReference) module;

        Initialized = true;
    }

    public async Task<bool> JoinRoomAsync(RoomTokenDto token, ChatModel chat)
    {
        if (!CheckInitialized())
        {
            return false;
        }

        return await _jsModule.InvokeAsync<bool>("Start", token.Token);
    }

    public async Task LeaveCallAsync()
    {
        if (!CheckInitialized()) return;

        await _jsModule.InvokeVoidAsync("Stop");
    }

    public void ChangeUserVolume(CallParticipantModel participant, Volume volume, AudioSource audioSource)
    {
        if (!CheckInitialized()) return;

        _jsModule.InvokeVoid("ChangeVolume", participant.Id, volume);
    }

    public async Task<bool> ToggleMicrophoneAsync()
    {
        if (!CheckInitialized())
        {
            return false;
        }

        return await _jsModule.InvokeAsync<bool>("ToggleMicrophoneEnabled");
    }

    [MemberNotNullWhen(true, nameof(_jsModule))]
    private bool CheckInitialized() => _jsModule is not null;

    [JSInvokable, EditorBrowsable(EditorBrowsableState.Never)]
    public void DisconnectedCallback(DisconnectReason reason)
    {
        _logger.LogInformation("Disconnected, reason: {Reason}", reason);
    }

    [JSInvokable, EditorBrowsable(EditorBrowsableState.Never)]
    public void ErrorCallback(string title, string message)
    {
        _logger.LogError("{Title}: {Message}", title, message);
    }

    [JSInvokable, EditorBrowsable(EditorBrowsableState.Never)]
    public void LocalParticipantStateUpdatedCallback(LocalParticipantState localParticipantState)
    {
        _logger.LogInformation("Local state updated");
    }

    [JSInvokable, EditorBrowsable(EditorBrowsableState.Never)]
    public void MicrophonesUpdatedCallback(MediaDeviceInfo[] microphonesInfo)
    {
        _mediaControlCallback?.MicrophoneListUpdated(microphonesInfo);
    }

    [JSInvokable, EditorBrowsable(EditorBrowsableState.Never)]
    public void CamerasUpdatedCallback(MediaDeviceInfo[] camerasInfo)
    {
        _mediaControlCallback?.CameraListUpdated(camerasInfo);
    }

    [JSInvokable, EditorBrowsable(EditorBrowsableState.Never)]
    public void ParticipantSpeakingStateChangedCallback(UserId userId, bool isSpeaking)
    {
        if (_connectionCallback?.GetParticipantById(userId) is { } participant)
        {
            participant.IsSpeaking = isSpeaking;
        }
    }

    [JSInvokable, EditorBrowsable(EditorBrowsableState.Never)]
    public void ParticipantUpdatedCallback(RemoteParticipantState state, GroupId groupId)
    {
        if (_connectionCallback?.GetParticipantById(state.Id) is { } participant)
        {
            participant.IsSpeaking = state.IsSpeaking;
            participant.ConnectionQuality = state.ConnectionQuality;
            participant.MicrophoneEnabled = state.MicrophoneOn;
        }
    }

    [JSInvokable, EditorBrowsable(EditorBrowsableState.Never)]
    public void ParticipantListReceivedCallback(RemoteParticipantState[] participants, GroupId groupId)
    {
        _connectionCallback?.ParticipantListReceived(participants, groupId);
    }

    [JSInvokable, EditorBrowsable(EditorBrowsableState.Never)]
    public void ParticipantConnectedCallback(RemoteParticipantState participant, GroupId groupId)
    {
        _connectionCallback?.ParticipantConnected(participant, groupId);
    }

    [JSInvokable, EditorBrowsable(EditorBrowsableState.Never)]
    public void ParticipantDisconnectedCallback(RemoteParticipantState participant, GroupId groupId)
    {
        _connectionCallback?.ParticipantDisconnected(participant, groupId);
    }

    public void Dispose()
    {
        _dotNetObjectReference.Dispose();
        _jsModule?.Dispose();
    }
}
