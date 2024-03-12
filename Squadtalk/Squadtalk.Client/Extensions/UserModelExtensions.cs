using Shared.Enums;
using Shared.Models;

namespace Squadtalk.Client.Extensions;

public static class UserModelExtensions
{
    public static string StatusString(this UserModel model) => model.Status switch
    {
        UserStatus.Online => "Online",
        UserStatus.Away => "Away",
        UserStatus.DoNotDisturb => "Do not disturb",
        UserStatus.Offline => "Offline",
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
    
    public static string StatusColor(this UserModel model) => model.Status switch
    {
        UserStatus.Online => "limegreen",
        UserStatus.Away => "darkorange",
        UserStatus.DoNotDisturb => "firebrick",
        UserStatus.Offline => "gray",
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
}