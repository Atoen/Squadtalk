using System.Collections.Immutable;
using Shared.Data.TypedIds;
using Squadtalk.Data.Entities;

namespace Squadtalk.Data;

public class VoiceCall
{
    public required List<VoiceUser> Users { get; init; }

    public required CallId Id { get; init; }

    public required ChannelId ChannelId { get; init; }

    public string GroupName => ChannelId.Value;
}

public record VoiceUser(ApplicationUser User, SignalRConnectionId ConnectionId);

public record VoiceCallOffer(VoiceUser Caller, ImmutableList<ApplicationUser> Callees, ChannelId ChannelId, CallOfferId Id);