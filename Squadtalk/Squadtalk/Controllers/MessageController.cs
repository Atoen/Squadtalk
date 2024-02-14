using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Shared.Communication;
using Shared.DTOs;
using Shared.Extensions;
using Squadtalk.Data;
using Squadtalk.Hubs;
using Squadtalk.Services;

namespace Squadtalk.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<MessageController> _logger;

    public MessageController(ApplicationDbContext dbContext, ILogger<MessageController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    private const int MessagePageCount = 20;

    private static readonly Func<ApplicationDbContext, string, IAsyncEnumerable<Message>> MessageFirstPageAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, string channelId) => context.Messages
                .AsNoTracking()
                .OrderByDescending(m => m.Timestamp)
                .Where(m => m.ChannelId == channelId)
                .Take(MessagePageCount)
                .Include(m => m.Author)
                .Reverse());

    private static readonly Func<ApplicationDbContext, string, DateTimeOffset, IAsyncEnumerable<Message>>
        MessagePageByCursorAsync =
            EF.CompileAsyncQuery(
                (ApplicationDbContext context, string channelId, DateTimeOffset cursor) => context.Messages
                    .AsNoTracking()
                    .OrderByDescending(m => m.Timestamp)
                    .Where(m => m.ChannelId == channelId)
                    .Where(m => m.Timestamp < cursor)
                    .Take(MessagePageCount)
                    .Include(m => m.Author)
                    .Reverse());

    private static readonly Func<ApplicationDbContext, string, Task<ApplicationUser?>> UserWithChannelsByIdAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, string userId) => context.Users
                .Include(x => x.Channels)
                .ThenInclude(x => x.Participants)
                .AsSplitQuery()
                .FirstOrDefault(x => x.Id == userId));

    [HttpGet("{channelId}/{timestamp?}")]
    public async IAsyncEnumerable<MessageDto> GetMessages(string channelId, string? timestamp)
    {
        var userId = HttpContext.User.GetRequiredClaimValue(ClaimTypes.NameIdentifier);
        var user = await UserWithChannelsByIdAsync(_dbContext, userId);
        if (user is null)
        {
            _logger.LogWarning("Cannot retrieve user data");
            HttpContext.Response.StatusCode = 500;
            yield break;
        }
        
        if (channelId != GroupChat.GlobalChatId && !user.Channels.Exists(x=> x.Id == channelId))
        {
            HttpContext.Response.StatusCode = 401;
            yield break;
        }

        var cursor = CreateCursor(timestamp);
        
        var messages = cursor == default
            ? MessageFirstPageAsync(_dbContext, channelId)
            : MessagePageByCursorAsync(_dbContext, channelId, cursor);
        
        await foreach (var message in messages)
        {
            yield return message.ToDto();
        }
    }
    
    [HttpPost("createChannel")]
    public async Task<IActionResult> CreateChannel(List<string> participantsId,
        [FromServices] IHubContext<ChatHub, IChatClient> hubContext,
        [FromServices] ChatConnectionManager<UserDto, string> connectionManager)
    {
        if (participantsId.Count < 2 || participantsId.Distinct().Count() != participantsId.Count)
        {
            return BadRequest();
        }

        var participants = await _dbContext.Users
            .Where(x => participantsId.Contains(x.Id))
            .ToListAsync();

        if (participants.Count != participantsId.Count)
        {
            return BadRequest();
        }

        var channel = new Channel
        {
            Id = Guid.NewGuid().ToString(),
            Participants = participants
        };

        await _dbContext.Channels.AddAsync(channel);
        await _dbContext.SaveChangesAsync();

        var dto = new ChannelDto
        {
            Id = channel.Id,
            Participants = participants.Select(x => x.ToDto()).ToList()
        };
        
        foreach (var userDto in dto.Participants)
        {
            var userConnections = connectionManager.GetUserConnections(userDto);
            foreach (var connection in userConnections)
            {
                await hubContext.Groups.AddToGroupAsync(connection, channel.Id);
                await hubContext.Clients.Client(connection).AddedToChannel(dto);
            }
        }

        return Ok(dto.Id);
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