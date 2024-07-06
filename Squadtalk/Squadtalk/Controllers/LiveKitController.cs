using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Squadtalk.Data.LiveKit.Events;
using Squadtalk.Hubs;
using Squadtalk.Services;

namespace Squadtalk.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class LiveKitController(
    VoiceCallManager voiceCallManager,
    IHubContext<ChatHub, IChatClient> hubContext) : ControllerBase
{
    [HttpPost("WebHook")]
    public async Task<IActionResult> HandleWebHook(JsonObject payload)
    {
        var liveKitEvent = LiveKitEvent.Create(payload);
        await voiceCallManager.HandleEventAsync(liveKitEvent, hubContext);

        return Ok();
    }
}
