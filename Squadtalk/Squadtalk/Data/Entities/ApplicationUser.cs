using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;

namespace Squadtalk.Data.Entities;

public class ApplicationUser : IdentityUser<UserId>, IChatUser
{
    public ICollection<GroupParticipant> GroupParticipants { get; set; } = default!;

    [NotMapped]
    public IEnumerable<Group> Groups => GroupParticipants.Select(x => x.Group);

    public DateTimeOffset LastSeen { get; set; }

    string IChatUser.Username => UserName!;

    UserStatus IChatUser.Status => UserStatus.Unknown;
}