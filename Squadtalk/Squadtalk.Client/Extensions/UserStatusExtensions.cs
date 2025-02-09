using MudBlazor;
using Shared.Enums;
using Squadtalk.Client.Localization;

namespace Squadtalk.Client.Extensions;

public static class UserStatusExtensions
{
    public static bool IsOfflineOrUnknown(this UserStatus status)
    {
        return status is UserStatus.Offline or UserStatus.Unknown;
    }

    public static string Name(this UserStatus status, LocalizedText.TextTable textTable) => status switch
    {
        UserStatus.Online => textTable.online,
        UserStatus.Away => textTable.away,
        UserStatus.DoNotDisturb => textTable.do_not_disturb,
        UserStatus.Offline => textTable.offline,
        _ => textTable.offline
    };

    private const string ColorPrefix = "color:";

    public static string ColorStyle(this UserStatus status) => status switch
    {
        UserStatus.Online => ColorPrefix + "var(--mud-palette-success)",
        UserStatus.Away => ColorPrefix + "var(--mud-palette-warning)",
        UserStatus.DoNotDisturb => ColorPrefix + "var(--mud-palette-error)",
        UserStatus.Offline => ColorPrefix + "var(--mud-palette-gray-dark)",
        _ => ColorPrefix + "var(--mud-palette-gray-dark)"
    };

    public static string StatusColor(this UserStatus status) => status switch
    {
        UserStatus.Online => "var(--mud-palette-success)",
        UserStatus.Away => "var(--mud-palette-warning)",
        UserStatus.DoNotDisturb => "var(--mud-palette-error)",
        UserStatus.Offline => "var(--mud-palette-gray-dark)",
        _ => "var(--mud-palette-gray-dark)"
    };

    public static string Icon(this UserStatus status) => status switch
    {
        UserStatus.Online => Icons.Material.Rounded.Circle,
        UserStatus.Away => Icons.Material.Rounded.AccessTimeFilled,
        UserStatus.DoNotDisturb => Icons.Material.Rounded.DoNotDisturbOn,
        _ => Icons.Material.TwoTone.Circle
    };

    public static string BadgeIcon(this UserStatus status) => status switch
    {
        UserStatus.DoNotDisturb => Icons.Material.Rounded.Remove,
        _ => string.Empty
    };

    public static Color PaletteColor(this UserStatus status) => status switch
    {
        UserStatus.Online => Color.Success,
        UserStatus.Away => Color.Warning,
        UserStatus.DoNotDisturb => Color.Error,
        _ => Color.Dark
    };
}
