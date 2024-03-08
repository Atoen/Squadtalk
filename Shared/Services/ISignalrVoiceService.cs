using Shared.Data;
using Shared.DTOs;

namespace Shared.Services;

public interface ISignalrVoiceService
{
    event Func<UserDto, CallOfferId, Task>? IncomingCall;
    event Func<CallOfferId, Task>? CallAccepted;
    event Func<CallOfferId, Task>? CallDeclined;
    event Func<CallId, Task>? CallEnded;
    event Func<string, Task>? CallFailed;
    event Func<List<UserDto>, CallId, Task>? GetCallUsers;
    
    Task<CallOfferId?> StartVoiceCallAsync(UserId id);

    Task EndCallAsync(CallId id);

    Task AcceptCallAsync(CallOfferId id);
    
    Task DeclineCallAsync(CallOfferId id);
}