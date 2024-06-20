using Microsoft.JSInterop;

namespace Squadtalk.Client.Extensions;

public static class JSExtensions
{
    public static ValueTask TryDisposeAsync(this IJSObjectReference? jsObjectReference)
    {
        return jsObjectReference?.DisposeAsync() ?? ValueTask.CompletedTask;
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

    public static async Task<IJSObjectReference> ImportAndInitModuleAsync2(
        this IJSRuntime jsRuntime,
        string moduleName,
        params object?[]? args)
    {
        foreach (var path in GetPathsToTry(moduleName))
        {
            var module = await TryImportModuleAsync(jsRuntime, path, args);
            if (module is not null)
            {
                return module;
            }
        }

        throw new InvalidOperationException($"Unable to load module {moduleName}");
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
    
    private static IEnumerable<string> GetPathsToTry(string moduleName)
    {
        yield return $"../Components/{moduleName}.razor.js";
        yield return $"../js/{moduleName}.min.js";
        yield return $"../js/{moduleName}.js";
    }

}