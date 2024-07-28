namespace Shared.Services;

public interface IMediaQueryService
{
    event Action MediaMatchesChanged;

    bool UseDesktopLayout { get; }

    bool UseMobileLayout => !UseDesktopLayout;

    Task InitializeAsync();
}
