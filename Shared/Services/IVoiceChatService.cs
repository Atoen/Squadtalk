namespace Shared.Services;

public interface IVoiceChatService : IAsyncDisposable
{
    bool Connected { get; }
    
    Task ConnectAsync();

    Task StartStreamAsync<T>(IAsyncEnumerable<T> stream, CancellationToken cancellationToken);

    IAsyncEnumerable<int> GetStream();
}