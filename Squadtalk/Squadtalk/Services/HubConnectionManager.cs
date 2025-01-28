using Shared.Data.TypedIds;
using Shared.Enums;
using StackExchange.Redis;

namespace Squadtalk.Services;

public class HubConnectionManager
{
    private readonly ILogger<HubConnectionManager> _logger;
    private readonly IDatabase _redisDb;

    private const string FCALL = nameof(FCALL);

    private static readonly object ZeroKeys = 0;

    private static readonly byte[] TypingUserChannelsPrefix = "user:typing:channels:"u8.ToArray();
    private static readonly byte[] UserConnectionsPrefix = "user:connections:"u8.ToArray();

    public HubConnectionManager(
        IConnectionMultiplexer connectionMultiplexer,
        ILogger<HubConnectionManager> logger)
    {
        _logger = logger;
        _redisDb = connectionMultiplexer.GetDatabase(2);
    }

    public async Task<bool> SetUserIsTypingAsync(GroupId groupId, UserId userId)
    {
        var result = (int) await _redisDb.ExecuteAsync(
            FCALL, "user_is_typing", ZeroKeys, groupId.Value, userId.ToString());

        const int shouldNotify = 1;
        return result == shouldNotify;
    }

    public async Task<bool> SetUserStoppedTyping(GroupId groupId, UserId userId)
    {
        var key = new RedisKey(userId.ToString()).Prepend(TypingUserChannelsPrefix);

        var userWasTypingOnChannel = await _redisDb.SetRemoveAsync(key, groupId.Value);

        return userWasTypingOnChannel;
    }

    public async Task<IEnumerable<string>> GetUserConnectionsAsync(UserId userId)
    {
        var key = new RedisKey(userId.ToString()).Prepend(UserConnectionsPrefix);
        var connections = await _redisDb.SetMembersAsync(key);

        return connections.Length != 0
            ? connections.Select(x => (string) x!)
            : [];
    }

    public async Task<(bool statusChanged, UserStatus currentStatus)> ConnectionStartedAsync(UserId userId, string connectionId)
    {
        var result = await _redisDb.ExecuteAsync(
        FCALL, "connection_started", ZeroKeys, userId.ToString(), connectionId);

        return ReadRedisResult(result);
    }

    public async Task<(bool statusChanged, UserStatus currentStatus)> ConnectionClosedAsync(UserId userId, string connectionId)
    {
        var result = await _redisDb.ExecuteAsync(
            FCALL, "connection_ended", ZeroKeys, userId.ToString(), connectionId);

        return ReadRedisResult(result);
    }

    public async Task<UserStatus> GetUserStatusAsync(UserId userId)
    {
        var result = await _redisDb.HashGetAsync("user:status", userId.ToString());
        return (UserStatus) (int) result;
    }

    public async Task<Dictionary<UserId, UserStatus>> GetUsersStatusAsync(IEnumerable<UserId> userIds)
    {
        var userIdsArray = userIds.ToArray();
        var redisKeys = userIdsArray.Select(id => (RedisValue) id.ToString()).ToArray();

        var results = await _redisDb.HashGetAsync("user:status", redisKeys);

        var statuses = new Dictionary<UserId, UserStatus>();
        for (var i = 0; i < userIdsArray.Length; i++)
        {
            var id = userIdsArray[i];
            statuses[id] = results[i].HasValue ? (UserStatus) (int) results[i] : UserStatus.Offline;
        }

        return statuses;
    }

    public async Task<(bool changed, UserStatus userStatus)> SetUserStatusAsync(UserId userId, UserStatus userStatus)
    {
        var result = await _redisDb.ExecuteAsync(
            FCALL, "set_user_status", ZeroKeys, userId.ToString(), (int) userStatus);

        return ReadRedisResult(result);
    }

    private static (bool, UserStatus) ReadRedisResult(RedisResult redisResult)
    {
        var data = (RedisResult[]?) redisResult;
        if (data is null)
        {
            return (false, UserStatus.Unknown);
        }

        var statusChanged = (bool) data[0];
        var currentStatus = (UserStatus) (int) data[1];

        return (statusChanged, currentStatus);
    }
}
