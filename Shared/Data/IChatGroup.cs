using Shared.Data.TypedIds;
using Shared.Enums;

namespace Shared.Data;

public interface IChatGroup
{
    GroupId Id { get; }

    string? CustomName { get; }

    IEnumerable<IGroupParticipant> Participants { get; }

    ChatType Type { get; }

    IChatMessage? LastMessage { get; }

    int MessagesSince { get; set; }
}