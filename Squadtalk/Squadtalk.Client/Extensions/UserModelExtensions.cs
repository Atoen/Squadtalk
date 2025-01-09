using Shared.Enums;
using Shared.Models;
using Squadtalk.Client.Localization;

namespace Squadtalk.Client.Extensions;

public static class UserModelExtensions
{
    public static string StatusString(this IStatus model, LocalizedText.TextTable textTable) => model.Status switch
    {
        UserStatus.Online => textTable.online,
        UserStatus.Away => textTable.away,
        UserStatus.DoNotDisturb => textTable.do_not_disturb,
        UserStatus.Offline => textTable.offline,
        _ => textTable.offline
    };

    private const string Color = "color:";
    
    public static string StatusColorStyle(this IStatus model) => model.Status switch
    {
        UserStatus.Online => Color + "var(--mud-palette-success)",
        UserStatus.Away => Color + "var(--mud-palette-warning)",
        UserStatus.DoNotDisturb => Color + "var(--mud-palette-error)",
        UserStatus.Offline => Color + "var(--mud-palette-gray-dark)",
        _ => Color + "var(--mud-palette-gray-dark)"
    };

    public static string StatusColor(this IStatus model) => model.Status switch
    {
        UserStatus.Online => "var(--mud-palette-success)",
        UserStatus.Away => "var(--mud-palette-warning)",
        UserStatus.DoNotDisturb => "var(--mud-palette-error)",
        UserStatus.Offline => "var(--mud-palette-gray-dark)",
        _ => "var(--mud-palette-gray-dark)"
    };
}
