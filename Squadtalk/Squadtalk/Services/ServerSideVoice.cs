using Shared.Data;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Services;

public class ServerSideVoice : IVoiceChatService
{
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public event Action? StateHasChanged;
    
    public Task StartCallAsync(UserModel invited)
    {
        throw new InvalidOperationException();
    }

    public Task EndCallAsync(CallId id)
    {
        throw new InvalidOperationException();
    }

    public Task AcceptCallAsync(CallOfferId id)
    {
        throw new InvalidOperationException();
    }

    public Task DeclineCallAsync(CallOfferId id)
    {
        throw new InvalidOperationException();
    }

    public VoiceCallModel? CurrentVoiceCall { get; }
    public event Func<UserModel, CallOfferId, Task>? CallOfferIncoming;
}