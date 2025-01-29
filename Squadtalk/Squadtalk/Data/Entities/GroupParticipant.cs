using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;

namespace Squadtalk.Data.Entities;

public class GroupParticipant : IGroupParticipant
{
    public UserId UserId { get; set; }
    public ChatUser User { get; set; } = default!;

    public GroupId GroupId { get; set; } = default!;
    public Group Group { get; set; } = default!;

    public ChatUser? AddedBy { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
    public GroupRole Role { get; set; }

    IChatUser? IGroupParticipant.AddedBy => AddedBy;
    string IChatUser.Username => User.Username;
    UserId IChatUser.Id => UserId;
    UserStatus IChatUser.Status => User.Status;
}
