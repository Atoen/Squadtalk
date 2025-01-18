using Microsoft.AspNetCore.SignalR;
using Shared.Data.TypedIds;
using Shared.Enums;
using Squadtalk.Data.Entities;
using Squadtalk.Signalr;
using StackExchange.Redis;

namespace Squadtalk.Services;

public class HubConnectionManager
{
    private readonly IHubContext<AppHub, IChatClient> _hubContext;
    private readonly ILogger<HubConnectionManager> _logger;
    private const string FCALL = "FCALL";
    private static readonly object ZeroKeys = 0;

    private readonly IDatabase _redisDb;
    private readonly Task _subscriberTask;

    public HubConnectionManager(
        IConnectionMultiplexer connectionMultiplexer,
        IHubContext<AppHub, IChatClient> hubContext,
        ILogger<HubConnectionManager> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
        _redisDb = connectionMultiplexer.GetDatabase(2);

        var subscriber = connectionMultiplexer.GetSubscriber();
        var eventChannel = RedisChannel.Literal("__keyevent@2__:expired");

        _subscriberTask = subscriber.SubscribeAsync(eventChannel, EventHandler);
    }

    public async Task<bool> SetUserIsTypingAsync(ChannelId channelId, UserId userId)
    {
        await _subscriberTask;

        var redisKey = $"user:typing:{userId}";
        var shadowKey = $"shadow:user:typing:{userId}";

        var keySet = await _redisDb.StringSetAsync(shadowKey, string.Empty, TimeSpan.FromSeconds(10));
        // No change in state
        if (!keySet)
        {
            return false;
        }

        // Allow the event handler to run for up to 5 sec and expire it automatically if it fails
        await _redisDb.StringSetAsync(redisKey, channelId.Value, TimeSpan.FromSeconds(15));

        return true;
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
        FCALL, "connection_started", ZeroKeys, user.Id.ToString(), connectionId);

        return ReadRedisResult(result);
    }
    
    public async Task<(bool statusChanged, UserStatus currentStatus)> ConnectionClosedAsync(ApplicationUser user, string connectionId)
    {
        var result = await _redisDb.ExecuteAsync(
            FCALL, "connection_ended", ZeroKeys, user.Id.ToString(), connectionId);

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

    // Subscribe async doesn't accept delegates returning Task
    private async void EventHandler(RedisChannel channel, RedisValue value)
    {
        try
        {
            var expiredKey = (string?) value;
            // The even could be raised for expiring the non-shadow key if the handler failed to remove it
            if (expiredKey is null || !expiredKey.StartsWith("shadow:user:typing"))
            {
                return;
            }

            var userId = expiredKey.Split(':').Last();
            var channelId = (string?) await _redisDb.StringGetDeleteAsync($"user:typing:{userId}");
            if (channelId is null)
            {
                return;
            }

            await _hubContext.Clients.Group(channelId)
                .UserStoppedTyping(ChannelId.From(channelId), UserId.Parse(userId));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when handling redis event");
        }
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
