using Shared.Data.TypedIds;
using Squadtalk.Client.Network;

namespace Squadtalk.Client.Services;

internal class CreateTextChannelRequestHandler(IMessageApi messageApi, ILogger<CreateTextChannelRequestHandler> logger)
{
    public async Task<ChannelId?> CreateTextChannelAsync(IEnumerable<UserId> participants)
    {
        try
        {
            var createdChannelId = await messageApi.CreateChannel(participants);
            return createdChannelId;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while sending create channel request");
            return null;
        }
    }
}
