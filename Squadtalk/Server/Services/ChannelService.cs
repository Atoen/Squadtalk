using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Squadtalk.Server.Models;

namespace Squadtalk.Server.Services;

public class ChannelService
{
    private readonly UserService _userService;
    private readonly AppDbContext _dbContext;
    
    private static readonly Guid GlobalChannelId = Guid.Empty;

    public ChannelService(UserService userService, AppDbContext dbContext)
    {
        _userService = userService;
        _dbContext = dbContext;
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
}