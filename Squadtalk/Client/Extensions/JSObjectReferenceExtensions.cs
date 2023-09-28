using Microsoft.JSInterop;

namespace Squadtalk.Client.Extensions;

public static class JSObjectReferenceExtensions
{
    public static async ValueTask TryDisposeAsync(this IJSObjectReference? jsObjectReference)
    {
        if (jsObjectReference is not null)
        {
            await jsObjectReference.DisposeAsync();
        }
    }
}