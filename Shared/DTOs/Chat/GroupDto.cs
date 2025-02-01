using MessagePack;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;

namespace Shared.DTOs.Chat;

[MessagePackObject(AllowPrivate = true)]
public class GroupDto : IChatGroup
{
    [Key(0)]
    public GroupId Id { get; set; } = default!;

    [Key(1)]
    public string? CustomName { get; set; }

    [Key(2)]
    public List<GroupParticipantDto> Participants { get; set; } = default!;

    [Key(3)]
    public MessageDto? LastMessage { get; set; }

    [Key(4)]
    public int MessagesSince { get; set; }

    [Key(5)]
    public ChatType Type { get; set; }

    [IgnoreMember]
    IEnumerable<IGroupParticipant> IChatGroup.Participants => Participants;

    [IgnoreMember]
    IChatMessage? IChatGroup.LastMessage => LastMessage;
}
