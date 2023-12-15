using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Communication;
using Shared.DTOs;
using Squadtalk.Data;

namespace Squadtalk.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<MessageController> _logger;

    public MessageController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, ILogger<MessageController> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _logger = logger;
    }
    
    private const int MessagePageCount = 20;
    
    public static readonly Func<ApplicationDbContext, string, IAsyncEnumerable<Message>> MessageFirstPageAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, string channelId) => context
                .Messages.OrderByDescending(m => m.Timestamp)
                .Where(m => m.ChannelId == channelId)
                .Take(MessagePageCount)
                .Include(m => m.Author)
                .Reverse());
    
    public static readonly Func<ApplicationDbContext, string, DateTimeOffset, IAsyncEnumerable<Message>>
        MessagePageByCursorAsync =
            EF.CompileAsyncQuery(
                (ApplicationDbContext context, string channelId, DateTimeOffset cursor) => context
                    .Messages.OrderByDescending(m => m.Timestamp)
                    .Where(m => m.ChannelId == channelId)
                    .Where(m => m.Timestamp < cursor)
                    .Take(MessagePageCount)
                    .Include(m => m.Author)
                    .Reverse());

    [HttpGet("{channelId}/{timestamp?}")]
    public async IAsyncEnumerable<MessageDto> GetMessages(string channelId, string? timestamp)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
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
    
    private static DateTimeOffset CreateCursor(string? timestamp)
    {
        if (timestamp is null) return default;

        Span<byte> buffer = stackalloc byte[timestamp.Length];

        if (!Convert.TryFromBase64String(timestamp, buffer, out var written)) return default;

        var converted = Encoding.UTF8.GetString(buffer[..written]);

        return long.TryParse(converted, out var ticks)
            ? new DateTimeOffset(ticks, TimeSpan.Zero)
            : default;
    }
}