using Squadtalk.Shared;

namespace Squadtalk.Server.Services;

public class ConnectionManager
{
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly Dictionary<UserDto, List<string>> _userConnectionIds = new();
    private readonly Dictionary<UserDto, uint> _userConnectionsCount = new();

    public IEnumerable<UserDto> ConnectedUsers => _userConnectionsCount.Keys;

    public IReadOnlyList<string> GetUserConnectionIds(UserDto user)
    {
        return _userConnectionIds[user];
    }

    public async Task<bool> UserConnected(UserDto user, string connectionId)
    {
        await _semaphore.WaitAsync();

        try
        {
            var userAlreadyConnected = _userConnectionsCount.TryGetValue(user, out var existingConnectionCount);
            if (userAlreadyConnected)
            {
                _userConnectionsCount[user] = existingConnectionCount + 1;
                _userConnectionIds[user].Add(connectionId);
            }
            else
            {
                _userConnectionsCount.Add(user, 1);
                _userConnectionIds[user] = new List<string> { connectionId };
            }

            return !userAlreadyConnected;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> UserDisconnected(UserDto user, string connectionId)
    {
        await _semaphore.WaitAsync();

        try
        {
            var existingConnectionCount = _userConnectionsCount[user];
            var isTheOnlyConnection = existingConnectionCount == 1;

            if (isTheOnlyConnection)
            {
                _userConnectionsCount.Remove(user);
                _userConnectionIds.Remove(user);
            }
            else
            {
                _userConnectionsCount[user] = existingConnectionCount - 1;
                _userConnectionIds[user].Remove(connectionId);
            }

            return isTheOnlyConnection;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}