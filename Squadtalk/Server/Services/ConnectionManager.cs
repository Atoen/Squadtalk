using Squadtalk.Shared;

namespace Squadtalk.Server.Services;

public class ConnectionManager
{
    private readonly List<UserDto> _connectedUsers = new();
    private readonly Dictionary<UserDto, uint> _connections = new();
    private readonly SemaphoreSlim _semaphore = new(1);
    
    public IReadOnlyList<UserDto> ConnectedUsers => _connectedUsers;

    public async Task<bool> UserConnected(UserDto user)
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

    public async Task<bool> UserDisconnected(UserDto user)
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