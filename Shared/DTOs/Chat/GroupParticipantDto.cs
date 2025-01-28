using MessagePack;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;

namespace Shared.DTOs.Chat;

[MessagePackObject(AllowPrivate = true)]
public class GroupParticipantDto : IGroupParticipant
{
    [Key(0)] public UserDto User { get; set; } = default!;

    [Key(1)] public GroupRole Role { get; set; }

    [Key(2)] public UserDto? AddedBy { get; set; }

    [IgnoreMember]
    IChatUser? IGroupParticipant.AddedBy => AddedBy;

    [IgnoreMember]
    public string Username => User.Username;

    [IgnoreMember]
    public UserId Id => User.Id;

    [IgnoreMember]
    public UserStatus Status => User.Status;
}
