using Microsoft.JSInterop;
using Microsoft.JSInterop.Implementation;
using Shared;
using Shared.Data.TypedIds;
using Shared.DTOs;
using Shared.Models;
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

    public bool Initialized { get; private set; }

    public VoiceChatInterop(
        IJSRuntime jsRuntime,
        ILogger<VoiceChatInterop> logger)
    {
        _jsRuntime = (JSInProcessRuntime) jsRuntime;
        _logger = logger;

        _dotNetObjectReference = DotNetObjectReference.Create(this);
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
        return await _jsModule!.InvokeAsync<bool>("Start", token.Token);
    }

    [JSInvokable]
    public void DisconnectedCallback(DisconnectReason reason)
    {
        _logger.LogInformation("Disconnected, reason: {Reason}", reason);
    }

    [JSInvokable]
    public void ErrorCallback(string title, string message)
    {
        _logger.LogError("{Title}: {Message}", title, message);
    }

    [JSInvokable]
    public void LocalParticipantStateUpdatedCallback(LocalParticipantState localParticipantState)
    {
        _logger.LogInformation("Local state updated");
    }

    [JSInvokable]
    public void MicrophonesUpdatedCallback(MediaDeviceInfo[] microphones)
    {
        _logger.LogInformation("Microphones updated");
    }

    [JSInvokable]
    public void CamerasUpdatedCallback(MediaDeviceModel[] cameras)
    {
        _logger.LogInformation("Cameras updated");
    }

    [JSInvokable]
    public void ParticipantUpdatedCallback(RemoteParticipantState participantState, GroupId groupId)
    {

    }

    [JSInvokable]
    public void ParticipantListReceivedCallback(RemoteParticipantState[] participants, GroupId groupId)
    {

    }

    [JSInvokable]
    public void ParticipantConnectedCallback(RemoteParticipantState participantState, GroupId groupId)
    {

    }

    [JSInvokable]
    public void ParticipantDisconnectedCallback(RemoteParticipantState participantState, GroupId groupId)
    {

    }

    public void Dispose()
    {
        _dotNetObjectReference.Dispose();
        _jsModule?.Dispose();
    }
}
