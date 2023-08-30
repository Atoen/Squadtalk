namespace Squadtalk.Client.Shared;

public sealed class InfiniteScrollingItemsProviderRequest
{
    public InfiniteScrollingItemsProviderRequest(int startIndex, CancellationToken cancellationToken)
    {
        StartIndex = startIndex;
        CancellationToken = cancellationToken;
    }

    public int StartIndex { get; }
    public CancellationToken CancellationToken { get; }
}

public delegate Task<IEnumerable<T>> ItemsProviderRequestDelegate<T>(InfiniteScrollingItemsProviderRequest request);