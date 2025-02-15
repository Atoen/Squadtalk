using Shared.Models;
using Shared.Services;
using Squadtalk.Client.Services.SignalR;

namespace Squadtalk.Client.Services.Rtc;

internal sealed class RtcConnectionService : IRtcConnectionService
{
    private readonly SignalrService _signalrService;
    private readonly VoiceChatInterop _voiceChatInterop;
    private readonly ILogger<RtcConnectionService> _logger;

    public bool ConnectedToCall { get; private set; }

    public RtcConnectionService(
        SignalrService signalrService,
        VoiceChatInterop voiceChatInterop,
        ILogger<RtcConnectionService> logger)
    {
        _signalrService = signalrService;
        _voiceChatInterop = voiceChatInterop;
        _logger = logger;
    }

    public async Task StartCallAsync(ChatModel chat)
    {
        await TryInitialize();

        var result = await _signalrService.StartVoiceCallAsync(chat.Id);
        if (result.ErrorOrValueIs(null))
        {
            _logger.LogError("Failed to start call");
            return;
        }

        var joined = await _voiceChatInterop.JoinRoomAsync(result.Value!, chat);
        if (!joined)
        {
            return;
        }

        ConnectedToCall = true;
        chat.State.HasActiveCall = true;
    }

    public Task AcceptCallAsync(ChatModel chat) => Task.CompletedTask;
    public Task DeclineCallAsync(ChatModel chat) => Task.CompletedTask;
    public Task LeaveCallAsync(ChatModel chat) => Task.CompletedTask;

    public async Task<bool> GroupHasActiveCallAsync(ChatModel chat)
    {
        var result = await _signalrService.GroupHasActiveCall(chat.Id);
        var hasActiveCall = result.ValueOr(false);

        chat.State.HasActiveCall = hasActiveCall;

        return hasActiveCall;
    }

    private async Task TryInitialize()
    {
        if (_voiceChatInterop.Initialized) return;

        var result = await _signalrService.GetRtcEndpoint();
        if (result.IsError) return;

        await _voiceChatInterop.InitializeAsync(result.Value);
    }
}
