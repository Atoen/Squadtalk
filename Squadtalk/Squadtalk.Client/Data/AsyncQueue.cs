using System.Threading.Tasks.Dataflow;

namespace Squadtalk.Client.Data;

public class AsyncQueue<T> : IAsyncEnumerable<T>
{
    private readonly SemaphoreSlim _enumerationSemaphore = new(1);
    private readonly BufferBlock<T> _bufferBlock = new();

    public int Count => _bufferBlock.Count;

    public void Enqueue(T item) => _bufferBlock.Post(item);
    
    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        await _enumerationSemaphore.WaitAsync(cancellationToken);
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                yield return await _bufferBlock.ReceiveAsync(cancellationToken);
            }
        }
        finally
        {
            _enumerationSemaphore.Release();
        }
    }
}