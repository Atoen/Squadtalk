using Microsoft.AspNetCore.Identity;
using Shared.Data;

namespace Squadtalk.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser<UserId>
{
    [PersonalData]
    public List<Channel> Channels { get; set; }

    [PersonalData]
    public List<ApplicationUser> Friends { get; set; }
}