namespace Squadtalk.Server.Services;

public class UserManager
{
    private readonly List<string> _connectedUsers = new();
    private readonly Dictionary<string, uint> _connections = new();
    private readonly SemaphoreSlim _semaphore = new(1);
    
    public IReadOnlyList<string> ConnectedUsers => _connectedUsers;

    public async Task<bool> UserConnected(string user)
    {
        await _semaphore.WaitAsync();

        try
        {
            var userAlreadyConnected = _connections.TryGetValue(user, out var existingConnectionCount);
            if (userAlreadyConnected)
            {
                _connections[user] = existingConnectionCount + 1;
            }
            else
            {
                _connections.Add(user, 1);
                _connectedUsers.Add(user);
            }

            return !userAlreadyConnected;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> UserDisconnected(string user)
    {
        await _semaphore.WaitAsync();

        try
        {
            var existingConnectionCount = _connections[user];
            var isTheOnlyConnection = existingConnectionCount == 1;
            
            if (isTheOnlyConnection)
            {
                _connectedUsers.Remove(user);
                _connections.Remove(user);
            }
            else
            {
                _connections[user] = existingConnectionCount - 1;
            }

            return isTheOnlyConnection;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}