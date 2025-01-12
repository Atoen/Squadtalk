namespace Shared.Services;

public interface IConnectionService
{
    event Action<ConnectionStatus>? ConnectionStatusChanged;

    ConnectionStatus ConnectionStatus { get; }

    bool Connected => ConnectionStatus == ConnectionStatus.Connected;

    Task ConnectAsync();

    Task<TimeSpan> MeasureConnectionDelayAsync();

    Task OnPersisting();
}

public enum ConnectionStatus
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting
}