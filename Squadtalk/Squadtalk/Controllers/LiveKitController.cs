using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared.Services;

namespace Squadtalk.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LiveKitController(ILiveKitService liveKitService, ILogger<LiveKitController> logger) : ControllerBase
{
    [HttpPost("CreateRoomToken")]
    public async Task<IActionResult> CreateRoomToken(CreateRoomRequestDto request)
    {
        var token = await liveKitService.CreateRoomTokenAsync(request.ChannelId);

        return token is null ? Problem() : Ok(token);
    }

    [HttpPost("WebHook")]
    public async Task<IActionResult> HandleWebHook([FromBody] JsonObject payload)
    {
        var eventName = payload["event"]!.ToString();
        var id = payload["id"]!.ToString();
        var createdAt = payload["createdAt"]!.ToString();
        var timestamp = long.Parse(createdAt);
        var createdAtTimestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).ToLocalTime();

        logger.LogInformation("Event: {Event} {Id} {CreatedAt}", eventName, id, createdAtTimestamp);

        return Ok();
    }
}