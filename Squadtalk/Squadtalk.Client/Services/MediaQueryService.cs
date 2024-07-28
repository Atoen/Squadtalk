using Microsoft.JSInterop;
using Shared;
using Shared.Services;
using Squadtalk.Client.Extensions;

namespace Squadtalk.Client.Services;

public sealed class MediaQueryService : IMediaQueryService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<MediaQueryService> _logger;

    private readonly DotNetObjectReference<MediaQueryService> _dotNetObjectReference;
    private IJSObjectReference? _jsModule;

    public bool UseDesktopLayout { get; private set; }

    public event Action? MediaMatchesChanged;

    public MediaQueryService(IJSRuntime jsRuntime, ILogger<MediaQueryService> logger)
    {
        _dotNetObjectReference = DotNetObjectReference.Create(this);
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _jsModule ??= await _jsRuntime.ImportAndInitModuleAsync(JsModule.MediaQuery, _dotNetObjectReference);
    }

    [JSInvokable]
    public void MediaMatchesChangedCallback(bool matches)
    {
        _logger.LogInformation("Media matches {Matches}", matches);

        UseDesktopLayout = matches;
        MediaMatchesChanged?.Invoke();
    }

    public ValueTask DisposeAsync()
    {
        _dotNetObjectReference.Dispose();
        return _jsModule.TryDisposeAsync();
    }
}
