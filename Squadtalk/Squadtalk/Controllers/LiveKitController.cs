using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared.Services;

namespace Squadtalk.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LiveKitController(ILiveKitService liveKitService) : ControllerBase
{
    [HttpPost("CreateRoomToken")]
    public async Task<IActionResult> CreateRoomToken(CreateRoomRequestDto request)
    {
        var token = await liveKitService.CreateRoomTokenAsync(request.ChannelId);

        return token is null ? Problem() : Ok(token);
    }
}
