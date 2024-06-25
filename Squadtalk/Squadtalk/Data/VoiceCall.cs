using Shared.Data.TypedIds;
using Squadtalk.Data.Entities;

namespace Squadtalk.Data;

public class VoiceCall
{
    public required List<VoiceUser> Users { get; init; }
    public required CallId Id { get; init; }

    public string GroupName => Id.Value;
}

public record VoiceUser(ApplicationUser User, SignalRConnectionId ConnectionId);

public record VoiceCallOffer(VoiceUser Caller, ApplicationUser Callee, CallOfferId Id);