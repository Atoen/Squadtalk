using Shared.Enums;
using Shared.Models;

namespace Squadtalk.Client.Extensions;

public static class UserModelExtensions
{
    public static string StatusString(this IStatus model /*, TextTable textTable*/ ) => model.Status switch
    {
        // UserStatus.Online => textTable.Online,
        // UserStatus.Away => textTable.Away,
        // UserStatus.DoNotDisturb => textTable.DoNotDisturb,
        // UserStatus.Offline => textTable.Offline,
        // _ => textTable.Offline
        _ => "Offline"
    };
    
    public static string StatusColor(this IStatus model) => model.Status switch
    {
        UserStatus.Online => "var(--mud-palette-success)",
        UserStatus.Away => "var(--mud-palette-warning)",
        UserStatus.DoNotDisturb => "var(--mud-palette-error)",
        UserStatus.Offline => "var(--mud-palette-text-secondary)",
        _ => "var(--mud-palette-text-secondary)"
    };
}
