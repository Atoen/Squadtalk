using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Data.TypedIds;
using Shared.Extensions;
using Shared.Models;
using Squadtalk.Data;
using Squadtalk.Data.Entities;
using Squadtalk.Services;

namespace Squadtalk.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ChannelCreator _channelCreator;
    private readonly ILogger<MessageController> _logger;

    public MessageController(
        ApplicationDbContext dbContext,
        ChannelCreator channelCreator,
        ILogger<MessageController> logger)
    {
        _dbContext = dbContext;
        _channelCreator = channelCreator;
        _logger = logger;
    }
    
    private const int MessagePageCount = 20;

    private static readonly Func<ApplicationDbContext, ChannelId, IAsyncEnumerable<Message>> MessageFirstPageAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, ChannelId id) => context.Messages
                .AsNoTracking()
                .OrderByDescending(m => m.Timestamp)
                .Where(m => m.ChannelId == id)
                .Take(MessagePageCount)
                .Include(m => m.Author)
                .Reverse());

    private static readonly Func<ApplicationDbContext, ChannelId, DateTimeOffset, IAsyncEnumerable<Message>>
        MessagePageByCursorAsync =
            EF.CompileAsyncQuery(
                (ApplicationDbContext context, ChannelId id, DateTimeOffset cursor) => context.Messages
                    .AsNoTracking()
                    .OrderByDescending(m => m.Timestamp)
                    .Where(m => m.ChannelId == id)
                    .Where(m => m.Timestamp < cursor)
                    .Take(MessagePageCount)
                    .Include(m => m.Author)
                    .Reverse());

    private static readonly Func<ApplicationDbContext, UserId, Task<ApplicationUser?>> UserWithChannelsByIdAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, UserId id) => context.Users
                .Include(x => x.Channels)
                .ThenInclude(x => x.Participants)
                .AsSplitQuery()
                .FirstOrDefault(x => x.Id == id));

    [HttpGet("{channelId}/{timestamp?}")]
    public async Task<IActionResult> GetMessages(ChannelId channelId, string? timestamp)
    {
        var userId = UserId.Parse(HttpContext.User.GetRequiredClaimValue(ClaimTypes.NameIdentifier));
        
        var user = await UserWithChannelsByIdAsync(_dbContext, userId);
        if (user is null)
        {
            _logger.LogWarning("Cannot retrieve user data");
            return Problem("Cannot retrieve user data");
        }
        
        if (channelId != GroupChatModel.GlobalChatId && !user.Channels.Exists(x=> x.Id == channelId))
        {
            return Unauthorized();
        }

        var cursor = CreateCursor(timestamp);
        var messages = cursor == default
            ? MessageFirstPageAsync(_dbContext, channelId)
            : MessagePageByCursorAsync(_dbContext, channelId, cursor);

        return Ok(messages);
    }
    
    [HttpPost("createChannel")]
    public async Task<IActionResult> CreateChannel(List<UserId> participantsId)
    {
        var userId = UserId.Parse(HttpContext.User.GetRequiredClaimValue(ClaimTypes.NameIdentifier));
        var user = await UserWithChannelsByIdAsync(_dbContext, userId);
        if (user is null)
        {
            _logger.LogWarning("Cannot retrieve user data");
            return Problem("Cannot retrieve user data");
        }

        var channelId = await _channelCreator.CreateChannelAsync(user, participantsId);
        
        return channelId == default
            ? BadRequest()
            : Ok(channelId);
    }
    
    private static DateTimeOffset CreateCursor(string? timestamp)
    {
        if (timestamp is null)
        {
            return default;
        }

        if (!timestamp.TryFromBase64(out var converted, true))
        {
            return default;
        }

        return long.TryParse(converted, out var ticks)
            ? new DateTimeOffset(ticks, TimeSpan.Zero)
            : default;
    }
}
