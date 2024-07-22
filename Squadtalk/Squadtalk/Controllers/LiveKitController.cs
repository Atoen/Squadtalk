using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Squadtalk.Data.LiveKit.Events;
using Squadtalk.Services;

namespace Squadtalk.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class LiveKitController(LiveKitEventHandler eventHandler) : ControllerBase
{
    [HttpPost("WebHook")]
    public async Task<IActionResult> HandleWebHook(JsonObject payload)
    {
        var liveKitEvent = LiveKitEvent.Create(payload);
        await eventHandler.HandleEventAsync(liveKitEvent);

        return Ok();
    }

    // [AllowAnonymous]
    // [HttpPost("token")]
    // public IActionResult CreateToken([FromBody] string username, [FromServices] LiveKitService liveKitService)
    // {
    //     var token = liveKitService.CreateRoomToken(username);
    //     return Ok(token);
    // }
}
