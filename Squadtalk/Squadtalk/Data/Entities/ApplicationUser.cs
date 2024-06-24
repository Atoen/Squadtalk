using Microsoft.AspNetCore.Identity;
using Shared.Data;
using Shared.Data.TypedIds;

namespace Squadtalk.Data.Entities;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser<UserId>, IChatUser
{
    [PersonalData]
    public List<Channel> Channels { get; set; }

    [PersonalData]
    public List<ApplicationUser> Friends { get; set; }
    
    string IChatUser.Username => UserName!;
}