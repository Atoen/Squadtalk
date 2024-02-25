using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Squadtalk.Hubs;

[Authorize]
public class VoiceChatHub(ILogger<VoiceChatHub> logger) : Hub<IVoiceChatClient>
{
    public async Task StartStream(IAsyncEnumerable<string> stream)
    {
        await foreach (var item in stream)
        {
            logger.LogInformation("Data: {Data}", item);
        }
    }
}