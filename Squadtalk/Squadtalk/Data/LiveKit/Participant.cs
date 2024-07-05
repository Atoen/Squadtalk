using System.Collections.Immutable;

namespace Squadtalk.Data.LiveKit;

public record Participant(
    string Sid,
    string Identity,
    ParticipantState State,
    DateTimeOffset JoinedAt,
    ImmutableList<ParticipantPermission> Permissions);

public record ParticipantPermission(string Name, bool Value);

public enum ParticipantState
{
    Active,
    Disconnected
}