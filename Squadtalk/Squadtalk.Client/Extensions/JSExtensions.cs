using FluentResults;
using Microsoft.JSInterop;
using Shared;
using Shared.Results;

namespace Squadtalk.Client.Extensions;

public static class JSExtensions
{
    public static ValueTask TryDisposeAsync(this IJSObjectReference? jsObjectReference)
    {
        return jsObjectReference?.DisposeAsync() ?? ValueTask.CompletedTask;
    }

    public static async ValueTask<Result> TryInvokeVoidAsync2(
        this IJSObjectReference? jsObjectReference,
        string identifier,
        params object?[]? args)
    {
        if (jsObjectReference is null)
        {
            return Result.Fail(new JsModuleNotLoadedError(identifier));
        }

        try
        {
            await jsObjectReference.InvokeVoidAsync(identifier, args);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(new JsInvocationError(identifier, e));
        }
    }

    public static async ValueTask<Result<T>> TryInvokeAsync2<T>(
        this IJSObjectReference? jsObjectReference,
        string identifier,
        params object?[]? args)
    {
        if (jsObjectReference is null)
        {
            return Result.Fail<T>(new JsModuleNotLoadedError(identifier));
        }

        try
        {
            var result = await jsObjectReference.InvokeAsync<T>(identifier, args);
            return Result.Ok(result);
        }
        catch (Exception e)
        {
            return Result.Fail<T>(new JsInvocationError(identifier, e));
        }
    }

    public static Result TryInvokeVoid(
        this IJSInProcessObjectReference? jsObjectReference,
        string identifier,
        params object?[]? args)
    {
        if (jsObjectReference is null)
        {
            return Result.Fail(new JsModuleNotLoadedError(identifier));
        }

        try
        {
            jsObjectReference.InvokeVoid(identifier, args);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(new JsInvocationError(identifier, e));
        }
    }

    public static Result<T> TryInvoke<T>(
        this IJSInProcessObjectReference? jsObjectReference,
        string identifier,
        params object?[]? args)
    {
        if (jsObjectReference is null)
        {
            return Result.Fail<T>(new JsModuleNotLoadedError(identifier));
        }

        try
        {
            var result = jsObjectReference.Invoke<T>(identifier, args);
            return Result.Ok(result);
        }
        catch (Exception e)
        {
            return Result.Fail<T>(new JsInvocationError(identifier, e));
        }
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
        foreach (var path in JsModule.GetPathsToTry(jsModule))
        {
            if (await TryImportModuleAsync(jsRuntime, path, args) is { } module)
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
}
