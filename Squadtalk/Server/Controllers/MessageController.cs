using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Squadtalk.Server.Hubs;
using Squadtalk.Server.Models;
using Squadtalk.Server.Services;
using Squadtalk.Shared;

namespace Squadtalk.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly ChannelService _channelService;
    private readonly AppDbContext _dbContext;

    public MessageController(AppDbContext dbContext, ChannelService channelService)
    {
        _dbContext = dbContext;
        _channelService = channelService;
    }

    [Authorize]
    [HttpGet("{channelId:guid}/{timestamp?}")]
    public async IAsyncEnumerable<MessageDto> GetMessages(Guid channelId, string? timestamp)
    {
        var participates = await _channelService.CheckIfUserParticipatesInChannel(HttpContext.User, channelId);
        if (!participates)
        {
            HttpContext.Response.StatusCode = 404;
            yield break;
        }

        var cursor = CreateCursor(timestamp);

        var messages = cursor == default
            ? CompiledQueries.MessageFirstPageAsync(_dbContext, channelId)
            : CompiledQueries.MessagePageByCursorAsync(_dbContext, channelId, cursor);

        await foreach (var message in messages)
        {
            yield return message.ToDto();
        }
    }

    [HttpPost("createChannel")]
    public async Task<IActionResult> CreateChannel(IEnumerable<Guid> participantsId,
        [FromServices] IHubContext<ChatHub> hubContext,
        [FromServices] ConnectionManager connectionManager)
    {
        var idList = participantsId.ToList();
        if (idList.Count < 2 || idList.Distinct().Count() != idList.Count) return BadRequest();

        var participants = await _dbContext.Users
            .Where(x => idList.Contains(x.Id))
            .ToListAsync();

        if (participants.Count != idList.Count) return BadRequest();

        var channel = new Channel
        {
            Id = Guid.NewGuid(),
            Participants = participants
        };

        await _dbContext.Channels.AddAsync(channel);
        await _dbContext.SaveChangesAsync();

        var dto = new ChannelDto
        {
            Id = channel.Id,
            Participants = participants.Select(x => x.ToDto()).ToList()
        };

        var channelIdString = channel.Id.ToString();
        
        foreach (var userDto in dto.Participants)
        {
            var userConnections = connectionManager.GetUserConnectionIds(userDto);
            foreach (var connection in userConnections)
            {
                await hubContext.Groups.AddToGroupAsync(connection, channelIdString);
                await hubContext.Clients.Client(connection).SendAsync("AddedToChannel", dto);
            }
        }

        return Ok(dto);
    }

    [Authorize]
    [HttpGet("getChannels")]
    public async Task<IActionResult> GetUserChannels()
    {
        var name = User.Identity!.Name!;
        var user = await CompiledQueries.UserByNameWithChannelsAsync(_dbContext, name);

        if (user is null) return BadRequest();

        return Ok(user.Channels);
    }

    private static DateTimeOffset CreateCursor(string? timestamp)
    {
        if (timestamp is null) return default;

        Span<byte> buffer = stackalloc byte[80];

        if (!Convert.TryFromBase64String(timestamp, buffer, out var written)) return default;

        var converted = Encoding.UTF8.GetString(buffer[..written]);

        return long.TryParse(converted, out var ticks)
            ? new DateTimeOffset(ticks, TimeSpan.Zero)
            : default;
    }
}