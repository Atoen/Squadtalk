using Microsoft.AspNetCore.Components;
using Shared.Data;

namespace Squadtalk.Client.Extensions;

public static class WeakRefCollectionExtensions
{
    public static void InvokeStateHasChanged(this WeakRefCollection<ComponentBase> componentCollection)
    {
        componentCollection.ForEach(x => ((IHandleEvent) x).HandleEventAsync(EventCallbackWorkItem.Empty, null));
    }
}
