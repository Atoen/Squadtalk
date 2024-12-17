namespace Shared.Services;

public interface IConnectionService
{
    event Func<string, Task>? ConnectionStatusChanged;

    string ConnectionStatus { get; }

    bool Connected { get; }

    Task ConnectAsync();

    Task<TimeSpan> MeasureConnectionDelayAsync();

    Task OnPersisting();
}