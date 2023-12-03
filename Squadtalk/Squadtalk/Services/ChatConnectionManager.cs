namespace Squadtalk.Services;

public class ChatConnectionManager<T> where T : notnull
{
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly Dictionary<T, HashSet<string>> _connections = [];

    public IReadOnlyCollection<T> ConnectedUsers => _connections.Keys;

    public IEnumerable<string> GetUserConnections(T key)
    {
        if (_connections.TryGetValue(key, out var connections))
        {
            return connections;
        }

        return Enumerable.Empty<string>();
    }

    public async Task<bool> Add(T key, string connectionId)
    {
        await _semaphore.WaitAsync();

        try
        {
            var alreadyConnected = _connections.TryGetValue(key, out var existingConnections);
            if (alreadyConnected)
            {
                existingConnections!.Add(connectionId);
            }
            else
            {
                _connections[key] = [connectionId];
            }

            return !alreadyConnected;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<bool> Remove(T key, string connectionId)
    {
        await _semaphore.WaitAsync();

        try
        {
            if (!_connections.TryGetValue(key, out var existingConnections) || existingConnections.Count == 0)
            {
                return false;
            }

            var isTheOnlyConnection = existingConnections.Count == 1;
            if (isTheOnlyConnection)
            {
                _connections.Remove(key);
            }
            else
            {
                existingConnections.Remove(connectionId);
            }

            return isTheOnlyConnection;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}