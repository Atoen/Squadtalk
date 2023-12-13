using Microsoft.JSInterop;

namespace Squadtalk.Client.Extensions;

public static class JSObjectReferenceExtensions
{
    public static ValueTask TryDisposeAsync(this IJSObjectReference? jsObjectReference)
    {
        if (jsObjectReference is not null)
        {
            return jsObjectReference.DisposeAsync();
        }
        
        return ValueTask.CompletedTask;
    }
}