using Microsoft.AspNetCore.Identity;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;

namespace Squadtalk.Data.Entities;

public class ApplicationUser : IdentityUser<UserId>, IChatUser
{
    public ICollection<GroupParticipant> GroupParticipants { get; set; } = default!;

    string IChatUser.Username => UserName!;

    UserStatus IChatUser.Status => UserStatus.Unknown;

    public DateTimeOffset LastSeen { get; set; }
}