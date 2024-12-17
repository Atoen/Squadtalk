using Shared.Enums;
using Shared.Models;
using Squadtalk.Client.Localization;

namespace Squadtalk.Client.Extensions;

public static class UserModelExtensions
{
    public static string StatusString(this IStatus model, LocalizedText.TextTable textTable ) => model.Status switch
    {
        UserStatus.Online => textTable.online,
        UserStatus.Away => textTable.away,
        UserStatus.DoNotDisturb => textTable.do_not_disturb,
        UserStatus.Offline => textTable.offline,
        _ => textTable.offline
    };
    
    public static string StatusColor(this IStatus model) => model.Status switch
    {
        UserStatus.Online => "var(--mud-palette-success)",
        UserStatus.Away => "var(--mud-palette-warning)",
        UserStatus.DoNotDisturb => "var(--mud-palette-error)",
        UserStatus.Offline => "var(--mud-palette-action-default)",
        _ => "var(--mud-palette-action-default)"
    };
}
