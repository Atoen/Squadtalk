using Shared.Data.TypedIds;
using Squadtalk.Data.Entities;
using StackExchange.Redis;

namespace Squadtalk.Services;

public class ChatConnectionManager
{
    private const string UserConnectionsKey = "user:connections";

    private readonly IDatabase _redisDb;

    public ChatConnectionManager(IConnectionMultiplexer connectionMultiplexer)
    {
        _redisDb = connectionMultiplexer.GetDatabase(2);
    }

    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly Dictionary<UserId, HashSet<string>> _connections = [];

    public List<ApplicationUser> ConnectedUsers { get; } = [];

    public async Task<IEnumerable<string>> GetUserConnectionsAsync(ApplicationUser user)
    {
        var key = GetUserConnectionsRedisKey(user.Id);
        var connections = await _redisDb.SetMembersAsync(key);

        return connections.Length != 0
            ? connections.Select(x => (string) x)
            : [];
    }

    public async Task<bool> AddAsync(ApplicationUser user, string connectionId)
    {
        var userKey = GetUserConnectionsRedisKey(user.Id);
        var added = await _redisDb.SetAddAsync(userKey, connectionId);

        return added;

        // await _semaphore.WaitAsync();
        //
        // try
        // {
        //     var id = user.Id;
        //     var alreadyConnected = _connections.TryGetValue(id, out var existingConnections);
        //     if (alreadyConnected)
        //     {
        //         existingConnections!.Add(connectionId);
        //     }
        //     else
        //     {
        //         ConnectedUsers.Add(user);
        //         _connections[id] = [connectionId];
        //     }
        //
        //     return !alreadyConnected;
        // }
        // finally
        // {
        //     _semaphore.Release();
        // }
    }
    
    public async Task<bool> Remove(ApplicationUser user, string connectionId)
    {
        var userKey = GetUserConnectionsRedisKey(user.Id);
        var removed = await _redisDb.SetRemoveAsync(userKey, connectionId);

        return removed;

        // await _semaphore.WaitAsync();
        //
        // try
        // {
        //     var id = user.Id;
        //     if (!_connections.TryGetValue(id, out var existingConnections) || existingConnections.Count == 0)
        //     {
        //         return false;
        //     }
        //
        //     var isTheOnlyConnection = existingConnections.Count == 1;
        //     if (isTheOnlyConnection)
        //     {
        //         ConnectedUsers.RemoveAll(x => x.Id == id);
        //         _connections.Remove(id);
        //     }
        //     else
        //     {
        //         existingConnections.Remove(connectionId);
        //     }
        //
        //     return isTheOnlyConnection;
        // }
        // finally
        // {
        //     _semaphore.Release();
        // }
    }

    private static string GetUserConnectionsRedisKey(UserId userId)
    {
        return $"{UserConnectionsKey}:{userId}";
    }
}