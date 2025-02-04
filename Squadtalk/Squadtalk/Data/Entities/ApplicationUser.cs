using Microsoft.AspNetCore.Identity;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;

namespace Squadtalk.Data.Entities;

public class ApplicationUser : IdentityUser<UserId>, IChatUser
{
    string IChatUser.Username => UserName!;

    UserStatus IChatUser.Status => UserStatus.Unknown;
}
