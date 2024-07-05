using System.Collections.Immutable;

namespace Squadtalk.Data.LiveKit;

public record Room(
    string Sid,
    string Name,
    EmptyTimeout EmptyTimeout,
    DepartureTimeout DepartureTimeout,
    DateTimeOffset CreationTime,
    string TurnPassword,
    ImmutableList<RoomCodec> EnabledCodecs,
    int Participants);

public readonly record struct EmptyTimeout(int Seconds);

public readonly record struct DepartureTimeout(int Seconds);

public readonly record struct RoomCodec(string MimeType);
