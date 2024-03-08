using Shared.Data;
using Shared.Models;

namespace Shared.Services;

public interface IVoiceChatService : IAsyncDisposable
{
    VoiceCallModel? CurrentVoiceCall { get; }
    
    event Func<UserModel, CallOfferId, Task>? CallOfferIncoming; 
    
    event Action? StateHasChanged;
    
    Task StartCallAsync(UserModel invited);

    Task EndCallAsync(CallId id);

    Task AcceptCallAsync(CallOfferId id);

    Task DeclineCallAsync(CallOfferId id);
}