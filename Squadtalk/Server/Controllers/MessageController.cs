using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Squadtalk.Server.Models;
using Squadtalk.Shared;

namespace Squadtalk.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly AppDbContext _context;

    public MessageController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize]
    public async IAsyncEnumerable<MessageDto> GetMessages(string? timestamp)
    {
        var cursor = CreateCursor(timestamp);

        var messages = cursor == default
            ? CompiledQueries.MessageFirstPageAsync(_context)
            : CompiledQueries.MessagePageByCursorAsync(_context, cursor);
        
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