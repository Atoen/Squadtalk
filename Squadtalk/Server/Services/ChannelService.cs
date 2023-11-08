using System.Security.Claims;
using Squadtalk.Server.Models;

namespace Squadtalk.Server.Services;

public class ChannelService
{
    private readonly UserService _userService;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ChannelService> _logger;

    public static readonly Guid GlobalChannelId = Guid.Empty;
    public static string GlobalChannelIdString { get; } = GlobalChannelId.ToString();

    public ChannelService(UserService userService, AppDbContext dbContext, ILogger<ChannelService> logger)
    {
        _userService = userService;
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<bool> CheckIfUserParticipatesInChannel(ClaimsPrincipal principal, Guid channelId)
    {
        if (channelId == GlobalChannelId)
        {
            return true;
        }

        var channel = await CompiledQueries.ChannelByIdAsync(_dbContext, channelId);
        if (channel is null)
        {
            return false;
        }
        
        var userResult = await _userService.GetUserAsync(principal);
        if (!userResult.IsT0)
        {
            return false;
        }

        var user = userResult.AsT0;

        return channel.Participants.Any(x => x.Id == user.Id);
    }

    public async Task<List<Channel>?> GetUserChannelsAsync(string username)
    {
        var user = await CompiledQueries.UserByNameWithChannelsAsync(_dbContext, username);

        if (user is not null)
        {
            return user.Channels;
        }
        
        _logger.LogError("Could not retrieve list of channels for user {User}", username);
        return null;
    }
}