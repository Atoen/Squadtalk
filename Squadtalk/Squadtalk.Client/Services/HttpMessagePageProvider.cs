using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Services;
using Squadtalk.Client.Network;

namespace Squadtalk.Client.Services;

public class HttpMessagePageProvider(IMessageApi messageApi, ILogger<HttpMessagePageProvider> logger)
    : IMessagePageProvider
{
    private readonly List<IChatMessage> _empty = [];
    
    public async Task<List<IChatMessage>> GetPageAsync(
        ChannelId channelId, TextChannelCursor cursor, CancellationToken cancellationToken)
    {
        try
        {
            var response = await messageApi.GetMessagePage(channelId, cursor, cancellationToken);

            return response is { Count: > 0 } ?
                response.Cast<IChatMessage>().ToList()
                : _empty;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while fetching message page");
            return _empty;
        }
    }
}