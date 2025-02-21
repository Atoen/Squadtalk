using MudBlazor;
using Shared.Models;

namespace Squadtalk.Client.Extensions;

public static class ConnectionQualityExtensions
{
    public static string Icon(this ConnectionQuality connectionQuality) => connectionQuality switch
    {
        ConnectionQuality.Excellent => Icons.Material.Rounded.NetworkWifi,
        ConnectionQuality.Good => Icons.Material.Rounded.NetworkWifi3Bar,
        ConnectionQuality.Poor => Icons.Material.Rounded.NetworkWifi2Bar,
        ConnectionQuality.Lost => Icons.Material.Rounded.SignalWifi0Bar,
        _ => Icons.Material.Rounded.Wifi
    };

    public static Color IconColor(this ConnectionQuality connectionQuality) => connectionQuality switch
    {
        ConnectionQuality.Excellent => Color.Success,
        ConnectionQuality.Good => Color.Warning,
        ConnectionQuality.Poor => Color.Error,
        ConnectionQuality.Lost => Color.Error,
        _ => Color.Info
    };
}
