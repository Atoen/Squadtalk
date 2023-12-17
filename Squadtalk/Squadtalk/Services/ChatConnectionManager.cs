namespace Squadtalk.Services;

public class ChatConnectionManager<TUser, TKey> where TUser : notnull where TKey : IEquatable<TKey>
{
    private readonly IConnectionKeyAccessor<TUser, TKey> _keyAccessor;
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly Dictionary<TKey, HashSet<string>> _connections = [];
    private readonly List<TUser> _connectedUsers = [];

    public IReadOnlyCollection<TUser> ConnectedUsers => _connectedUsers;

    public ChatConnectionManager(IConnectionKeyAccessor<TUser, TKey> keyAccessor)
    {
        _keyAccessor = keyAccessor;
    }

    public IEnumerable<string> GetUserConnections(TUser user)
    {
        var key = _keyAccessor.GetKey(user);
        if (_connections.TryGetValue(key, out var connections))
        {
            return connections;
        }

        return Enumerable.Empty<string>();
    }

    public async Task<bool> Add(TUser user, string connectionId)
    {
        await _semaphore.WaitAsync();

        try
        {
            var key = _keyAccessor.GetKey(user);
            var alreadyConnected = _connections.TryGetValue(key, out var existingConnections);
            if (alreadyConnected)
            {
                existingConnections!.Add(connectionId);
            }
            else
            {
                _connectedUsers.Add(user);
                _connections[key] = [connectionId];
            }

            return !alreadyConnected;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<bool> Remove(TUser user, string connectionId)
    {
        await _semaphore.WaitAsync();

        try
        {
            var key = _keyAccessor.GetKey(user);
            if (!_connections.TryGetValue(key, out var existingConnections) || existingConnections.Count == 0)
            {
                return false;
            }

            var isTheOnlyConnection = existingConnections.Count == 1;
            if (isTheOnlyConnection)
            {
                _connectedUsers.RemoveAll(x => _keyAccessor.GetKey(x).Equals(key));
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