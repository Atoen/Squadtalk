using System.Text;
using Microsoft.AspNetCore.Mvc;
using Squadtalk.Server.Models;
using Squadtalk.Server.Services;
using Squadtalk.Shared;

namespace Squadtalk.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ChannelService _channelService;

    public MessageController(AppDbContext context, ChannelService channelService)
    {
        _context = context;
        _channelService = channelService;
    }

    // [Authorize]
    [HttpGet("{channelId:guid}/{timestamp?}")]
    public async IAsyncEnumerable<MessageDto> GetMessages(Guid channelId, string? timestamp)
    {
        var participates = await _channelService.CheckIfUserParticipatesInChannel(HttpContext.User, channelId);
        if (!participates)
        {
            yield break;
        }
        
        var cursor = CreateCursor(timestamp);
        
        var messages = cursor == default
            ? CompiledQueries.MessageFirstPageAsync(_context, channelId)
            : CompiledQueries.MessagePageByCursorAsync(_context, channelId, cursor);
        
        await foreach (var message in messages)
        {
            yield return message.ToDto();
        }
    }

    private static DateTimeOffset CreateCursor(string? timestamp)
    {
        if (timestamp is null)
        {
            return default;
        }

        Span<byte> buffer = stackalloc byte[80];

        if (!Convert.TryFromBase64String(timestamp, buffer, out var written))
        {
            return default;
        }

        var converted = Encoding.UTF8.GetString(buffer[..written]);

        return long.TryParse(converted, out var ticks)
            ? new DateTimeOffset(ticks, TimeSpan.Zero)
            : default;
    }
}