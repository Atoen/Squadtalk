using Microsoft.JSInterop;
using Shared;

namespace Squadtalk.Client.Extensions;

public static class JSExtensions
{
    public static ValueTask TryDisposeAsync(this IJSObjectReference? jsObjectReference)
    {
        return jsObjectReference?.DisposeAsync() ?? ValueTask.CompletedTask;
    }

    public static ValueTask TryInvokeVoidAsync(
        this IJSObjectReference? jsObjectReference,
        string identifier,
        params object?[]? args)
    {
        return jsObjectReference?.InvokeVoidAsync(identifier, args) ?? ValueTask.CompletedTask;
    }

    public static ValueTask<T> TryInvokeAsync<T>(
        this IJSObjectReference? jsObjectReference,
        string identifier,
        params object?[]? args)
    {
        return jsObjectReference?.InvokeAsync<T>(identifier, args) ?? default;
    }

    public static async Task<IJSObjectReference> ImportAndInitModuleAsync(
        this IJSRuntime jsRuntime,
        string modulePath,
        params object?[]? args)
    {
        var module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", modulePath);
        await module.InvokeVoidAsync("Init", args);

        return module;
    }

    public static async Task<IJSObjectReference> ImportAndInitModuleAsync(
        this IJSRuntime jsRuntime,
        JsModule jsModule,
        params object?[]? args)
    {
        foreach (var path in GetPathsToTry(jsModule))
        {
            var module = await TryImportModuleAsync(jsRuntime, path, args);
            if (module is not null)
            {
                return module;
            }
        }

        throw new InvalidOperationException($"Unable to load module {jsModule.Name}");
    }

    public static async Task<IJSObjectReference?> TryImportModuleAsync(
        IJSRuntime jsRuntime,
        string path,
        params object?[]? args)
    {
        try
        {
            var module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", path);
            await module.InvokeVoidAsync("Init", args);
            
            Console.WriteLine($"Loaded module {path}");
            
            return module;
        }
        catch
        {
            Console.WriteLine($"Failed to load module {path}");
            return null;
        }
    }

    private static IEnumerable<string> GetPathsToTry(JsModule jsModule)
    {
        switch (jsModule.Location)
        {
            case JsModuleLocation.Collocated:
                yield return $"../Components/{jsModule.Name}.razor.js";
                break;
            
            case JsModuleLocation.ScriptsFolder:
                yield return $"../js/{jsModule.Name}.js";
                break;
            
            case JsModuleLocation.ScriptsFolderMinified:
                yield return $"../js/{jsModule.Name}.min.js";
                break;
            
            case JsModuleLocation.Unspecified:
            default:
                yield return $"../Components/{jsModule.Name}.razor.js";
                yield return $"../js/{jsModule.Name}.min.js";
                yield return $"../js/{jsModule.Name}.js";
                break;
        }
    }
}
