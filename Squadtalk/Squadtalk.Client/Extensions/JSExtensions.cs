using Microsoft.JSInterop;

namespace Squadtalk.Client.Extensions;

public static class JSExtensions
{
    public static ValueTask TryDisposeAsync(this IJSObjectReference? jsObjectReference)
    {
        if (jsObjectReference is not null)
        {
            return jsObjectReference.DisposeAsync();
        }
        
        return ValueTask.CompletedTask;
    }

    public static async Task<IJSObjectReference> ImportAndInitModuleAsync(this IJSRuntime jsRuntime, string modulePath, params object?[]? args)
    {
        var module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", modulePath);
        await module.InvokeVoidAsync("Init", args);

        return module;
    }
}