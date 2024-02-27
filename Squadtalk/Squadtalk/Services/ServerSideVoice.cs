using Shared.Services;

namespace Squadtalk.Services;

public class ServerSideVoice : IVoiceChatService
{
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public bool Connected { get; }
    public Task ConnectAsync()
    {
        return Task.CompletedTask;
    }

    public Task StartStreamAsync<T>(IAsyncEnumerable<T> stream, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public IAsyncEnumerable<int> GetStream()
    {
        throw new NotImplementedException();
    }
}