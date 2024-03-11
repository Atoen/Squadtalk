using Microsoft.JSInterop;
using Shared.Data;
using Shared.DTOs;
using Shared.Extensions;
using Shared.Models;
using Shared.Services;
using Squadtalk.Client.Data;
using Squadtalk.Client.Extensions;

namespace Squadtalk.Client.Services;

public sealed class VoiceChatService : IVoiceChatService
{
    private readonly ISignalrVoiceService _signalrVoiceService;
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<VoiceChatService> _logger;

    private readonly DotNetObjectReference<VoiceChatService> _dotNetObject;
    private readonly AsyncQueue<byte[]> _voiceDataQueue = new();
    private IJSObjectReference? _jsModule;
    private CancellationTokenSource _cancellationTokenSource = new();
    
    public event Func<UserModel, CallOfferId, Task>? CallOfferIncoming;
    public event Action? StateHasChanged;
    
    public VoiceCallModel? CurrentVoiceCall { get; private set; }

    public VoiceChatService(ISignalrService signalrService, IJSRuntime jsRuntime, ILogger<VoiceChatService> logger)
    {
        _signalrVoiceService = signalrService;
        _jsRuntime = jsRuntime;
        _logger = logger;
        
        _signalrVoiceService.IncomingCall += IncomingCall;
        _signalrVoiceService.CallAccepted += CallAccepted;
        _signalrVoiceService.CallDeclined += CallDeclined;
        _signalrVoiceService.CallEnded += CallEnded;
        _signalrVoiceService.CallFailed += CallFailed;
        _signalrVoiceService.GetCallUsers += GetCallUsers;
        _signalrVoiceService.GetVoicePacket += GetVoicePacket;

        _dotNetObject = DotNetObjectReference.Create(this);
    }

    private async Task InitializeModuleAsync()
    {
        _jsModule ??= await _jsRuntime.ImportAndInitModuleAsync("../js/VoiceChat.js", _dotNetObject);
    }

    private async Task GetVoicePacket(VoicePacketDto packet)
    {
        var base64String = Convert.ToBase64String(packet.Data);
        var speaker = CurrentVoiceCall?.Connected.FirstOrDefault(x => x.Id == packet.Id.Value);
        _logger.LogInformation("{User} is speaking", speaker?.Username);
        
        try
        {
            await _jsModule!.InvokeVoidAsync("PlayAudio", "data:audio/webm;base64," + base64String).AsTask();
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Error while playing audio");
        }
    }
    
    private async Task StartStreaming()
    {
        await _jsModule!.InvokeVoidAsync("StartRecorder");
        
        await _signalrVoiceService.StreamDataAsync((CallId) CurrentVoiceCall!.Id, _voiceDataQueue,
            _cancellationTokenSource.Token);
    }

    private Task GetCallUsers(List<UserDto> users, CallId id)
    {
        var voiceCallModel = new VoiceCallModel
        {
            Connected = users.Select(UserModel.GetOrCreate).ToList(),
            Id = id.Value
        };

        CurrentVoiceCall = voiceCallModel;
        StateHasChanged?.Invoke();

        return StartStreaming();
    }
    
    private Task CallFailed(string reason)
    {
        _logger.LogError("Call failed: {Reason}", reason);
        return Task.CompletedTask;
    }

    private async Task CallEnded(CallId id)
    {
        _logger.LogInformation("Call ended, id: {Id}", id);
        await _jsModule!.InvokeVoidAsync("StopRecorder");

        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        CurrentVoiceCall = null;
        StateHasChanged?.Invoke();
    }
    
    private Task IncomingCall(UserDto caller, CallOfferId id)
    {
        _logger.LogInformation("Incoming call from: {Caller}", caller.Username);
        return CallOfferIncoming.TryInvoke(UserModel.GetOrCreate(caller), id);
    }
    
    private Task CallDeclined(CallOfferId id)
    {
        _logger.LogInformation("Call declined, id: {Id}", id);
        return Task.CompletedTask;
    }
    
    private Task CallAccepted(CallOfferId id)
    {
        _logger.LogInformation("Call accepted, id: {Id}", id);
        return Task.CompletedTask;
    }

    public async Task StartCallAsync(UserModel callee)
    {
        await InitializeModuleAsync();
        var callOfferId = await _signalrVoiceService.StartVoiceCallAsync((UserId) callee.Id);
        if (callOfferId is null)
        {
            _logger.LogError("Call offer id is null");
            return;
        }
        
        _logger.LogInformation("Call offer id: {Id}", callOfferId);
    }

    public async Task EndCallAsync(CallId id)
    {
        CurrentVoiceCall = null;
        await _jsModule!.InvokeVoidAsync("StopRecorder");
        
        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        
        StateHasChanged?.Invoke();
        
        await _signalrVoiceService.EndCallAsync(id);
    }

    public async Task AcceptCallAsync(CallOfferId id)
    {
        await InitializeModuleAsync();
        await _signalrVoiceService.AcceptCallAsync(id);
    }

    public Task DeclineCallAsync(CallOfferId id)
    {
        return _signalrVoiceService.DeclineCallAsync(id);
    }

    [JSInvokable]
    public void MicDataCallback(string data)
    {
        _logger.LogInformation("Data: {Data}", data);
        
        var parts = data.Split(',');
        
        var bytes = Convert.FromBase64String(parts[1]);
        _voiceDataQueue.Enqueue(bytes);
    }

    public ValueTask DisposeAsync()
    {
        _dotNetObject.Dispose();
        return _jsModule.TryDisposeAsync();
    }
}