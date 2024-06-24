using Shared.Data.TypedIds;
using Squadtalk.Data.Entities;

namespace Squadtalk.Services;

public class ChatConnectionManager
{
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly Dictionary<UserId, HashSet<string>> _connections = [];

    public List<ApplicationUser> ConnectedUsers { get; } = [];

    public IEnumerable<string> GetUserConnections(ApplicationUser user)
    {
        if (_connections.TryGetValue(user.Id, out var connections))
        {
            return connections;
        }

        return Enumerable.Empty<string>();
    }

    public async Task<bool> Add(ApplicationUser user, string connectionId)
    {
        await _semaphore.WaitAsync();

        try
        {
            var id = user.Id;
            var alreadyConnected = _connections.TryGetValue(id, out var existingConnections);
            if (alreadyConnected)
            {
                existingConnections!.Add(connectionId);
            }
            else
            {
                ConnectedUsers.Add(user);
                _connections[id] = [connectionId];
            }

            return !alreadyConnected;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<bool> Remove(ApplicationUser user, string connectionId)
    {
        await _semaphore.WaitAsync();

        try
        {
            var id = user.Id;
            if (!_connections.TryGetValue(id, out var existingConnections) || existingConnections.Count == 0)
            {
                return false;
            }

            var isTheOnlyConnection = existingConnections.Count == 1;
            if (isTheOnlyConnection)
            {
                ConnectedUsers.RemoveAll(x => x.Id == id);
                _connections.Remove(id);
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