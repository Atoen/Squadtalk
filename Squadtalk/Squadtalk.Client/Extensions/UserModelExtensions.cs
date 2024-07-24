using Shared.Enums;
using Shared.Models;
using Squadtalk.Client.Localization;

namespace Squadtalk.Client.Extensions;

public static class UserModelExtensions
{
    public static string StatusString(this IStatus model, TextTable textTable) => model.Status switch
    {
        UserStatus.Online => textTable.Online,
        UserStatus.Away => textTable.Away,
        UserStatus.DoNotDisturb => textTable.DoNotDisturb,
        UserStatus.Offline => textTable.Offline,
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
    
    public static string StatusColor(this IStatus model) => model.Status switch
    {
        UserStatus.Online => "limegreen",
        UserStatus.Away => "darkorange",
        UserStatus.DoNotDisturb => "firebrick",
        UserStatus.Offline => "gray",
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
}
