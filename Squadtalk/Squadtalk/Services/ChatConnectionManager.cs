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

    public List<ApplicationUser> ConnectedUsers { get; } = [];

    public async Task<IEnumerable<string>> GetUserConnectionsAsync(ApplicationUser user)
    {
        var key = GetUserConnectionsRedisKey(user.Id);
        var connections = await _redisDb.SetMembersAsync(key);

        return connections.Length != 0
            ? connections.Select(x => (string) x!)
            : [];
    }

    public async Task<bool> AddAsync(ApplicationUser user, string connectionId)
    {
        var userKey = GetUserConnectionsRedisKey(user.Id);
        var added = await _redisDb.SetAddAsync(userKey, connectionId);

        return added;
    }
    
    public async Task<bool> RemoveAsync(ApplicationUser user, string connectionId)
    {
        var userKey = GetUserConnectionsRedisKey(user.Id);
        var removed = await _redisDb.SetRemoveAsync(userKey, connectionId);

        return removed;
    }

    private static string GetUserConnectionsRedisKey(UserId userId)
    {
        return $"{UserConnectionsKey}:{userId}";
    }
}