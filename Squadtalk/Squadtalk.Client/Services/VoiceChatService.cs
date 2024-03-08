using Shared.Data;
using Shared.DTOs;
using Shared.Extensions;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Client.Services;

public sealed class VoiceChatService : IVoiceChatService
{
    private readonly ISignalrVoiceService _signalrVoiceService;
    private readonly ILogger<VoiceChatService> _logger;

    public event Func<UserModel, CallOfferId, Task>? CallOfferIncoming;
    public event Action? StateHasChanged;
    
    public VoiceCallModel? CurrentVoiceCall { get; private set; }

    public VoiceChatService(ISignalrService signalrService, ILogger<VoiceChatService> logger)
    {
        _signalrVoiceService = signalrService;
        _logger = logger;
        
        _signalrVoiceService.IncomingCall += IncomingCall;
        _signalrVoiceService.CallAccepted += CallAccepted;
        _signalrVoiceService.CallDeclined += CallDeclined;
        _signalrVoiceService.CallEnded += CallEnded;
        _signalrVoiceService.CallFailed += CallFailed;
        _signalrVoiceService.GetCallUsers += SignalrVoiceServiceOnGetCallUsers;
    }
    
    private Task SignalrVoiceServiceOnGetCallUsers(List<UserDto> users, CallId id)
    {
        var voiceCallModel = new VoiceCallModel
        {
            Connected = users.Select(UserModel.GetOrCreate).ToList(),
            Id = id.Value
        };

        CurrentVoiceCall = voiceCallModel;
        StateHasChanged?.Invoke();
        
        return Task.CompletedTask;
    }
    
    private Task CallFailed(string reason)
    {
        _logger.LogError("Call failed: {Reason}", reason);
        return Task.CompletedTask;
    }

    private Task CallEnded(CallId id)
    {
        _logger.LogInformation("Call ended, id: {Id}", id);
        
        CurrentVoiceCall = null;
        StateHasChanged?.Invoke();
        
        return Task.CompletedTask;
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
        var callOfferId = await _signalrVoiceService.StartVoiceCallAsync((UserId) callee.Id);

        if (callOfferId is null)
        {
            _logger.LogError("Call offer id is null");
            return;
        }
        
        _logger.LogInformation("Call offer id: {Id}", callOfferId);
    }

    public Task EndCallAsync(CallId id)
    {
        CurrentVoiceCall = null;
        
        StateHasChanged?.Invoke();
        
        return _signalrVoiceService.EndCallAsync(id);
    }

    public Task AcceptCallAsync(CallOfferId id)
    {
        return _signalrVoiceService.AcceptCallAsync(id);
    }

    public Task DeclineCallAsync(CallOfferId id)
    {
        return _signalrVoiceService.DeclineCallAsync(id);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}