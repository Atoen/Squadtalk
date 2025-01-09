using Shared.Data.TypedIds;
using Shared.Enums;
using Squadtalk.Data.Entities;
using StackExchange.Redis;

namespace Squadtalk.Services;

public class HubConnectionManager
{
    private readonly IDatabase _redisDb;

    public HubConnectionManager(IConnectionMultiplexer connectionMultiplexer)
    {
        _redisDb = connectionMultiplexer.GetDatabase(2);
    }

    public async Task<IEnumerable<string>> GetUserConnectionsAsync(ApplicationUser user)
    {
        var key = $"user:connections:{user.Id}";
        var connections = await _redisDb.SetMembersAsync(key);

        return connections.Length != 0
            ? connections.Select(x => (string) x!)
            : [];
    }

    public async Task<(bool statusChanged, UserStatus currentStatus)> ConnectionStartedAsync(ApplicationUser user, string connectionId)
    {
        var result = await _redisDb.ExecuteAsync(
        "FCALL", "connection_started", 0, user.Id.ToString(), connectionId);

        return ReadRedisResult(result);
    }
    
    public async Task<(bool statusChanged, UserStatus currentStatus)> ConnectionClosedAsync(ApplicationUser user, string connectionId)
    {
        var result = await _redisDb.ExecuteAsync(
            "FCALL", "connection_ended", 0, user.Id.ToString(), connectionId);

        return ReadRedisResult(result);
    }

    public async Task<UserStatus> GetUserStatusAsync(UserId userId)
    {
        var result = await _redisDb.HashGetAsync("user:status", userId.Value.ToString());
        return (UserStatus) (int) result;
    }

    private (bool, UserStatus) ReadRedisResult(RedisResult redisResult)
    {
        var data = (RedisResult[]?) redisResult;
        if (data is not null)
        {
            var statusChanged = (int) data[0] == 1;
            var currentStatus = (UserStatus) (int) data[1];

            return (statusChanged, currentStatus);
        }

        return (false, UserStatus.Unknown);
    }
}