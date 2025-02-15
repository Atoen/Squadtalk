using Shared.Data.TypedIds;
using StackExchange.Redis;

namespace Squadtalk.Services;

public class RtcConnectionManager
{
    private readonly IDatabase _redisDb;
    private readonly ILogger<RtcConnectionManager> _logger;

    private static readonly RedisKey RoomsKey = "rooms"u8.ToArray();

    public RtcConnectionManager(
        IConnectionMultiplexer connectionMultiplexer,
        ILogger<RtcConnectionManager> logger)
    {
        _redisDb = connectionMultiplexer.GetDatabase(0);
        _logger = logger;
    }

    public async Task<bool> ChannelHasActiveCallAsync(GroupId groupId)
    {
        return await _redisDb.HashExistsAsync(RoomsKey, groupId.Value);
    }
}
