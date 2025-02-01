using System.ComponentModel.DataAnnotations.Schema;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;

namespace Squadtalk.Data.Entities;

public class ChatUser : IChatUser
{
    public ICollection<GroupParticipant> GroupParticipants { get; set; } = default!;

    [NotMapped]
    public IEnumerable<Group> Groups => GroupParticipants.Select(x => x.Group);

    public string Username { get; set; } = default!;

    [Column(nameof(ApplicationUser.Id))]
    public UserId Id { get; set; }

    public DateTimeOffset LastSeen { get; set; }

    // DB doesn't store user status
    public UserStatus Status => UserStatus.Unknown;
}
