using System.ComponentModel.DataAnnotations.Schema;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;

namespace Squadtalk.Data.Entities;

public class GroupParticipant : IGroupParticipant
{
    [ForeignKey(nameof(User))]
    public UserId UserId { get; set; }
    public ChatUser User { get; set; } = default!;

    [ForeignKey(nameof(Group))]
    public GroupId GroupId { get; set; } = default!;
    public Group Group { get; set; } = default!;

    [ForeignKey(nameof(AddedBy))]
    public UserId AddedById { get; set; }
    public ChatUser AddedBy { get; set; } = default!;

    public DateTimeOffset JoinedAt { get; set; }
    public GroupRole Role { get; set; }
    public MessageId LastMessageSeenId { get; set; }

    IChatUser IGroupParticipant.AddedBy => AddedBy;
    string IChatUser.Username => User.Username;
    UserId IChatUser.Id => UserId;
    UserStatus IChatUser.Status => User.Status;
}
