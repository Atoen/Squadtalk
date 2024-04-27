using RestSharp;
using Shared.Data;
using Shared.DTOs;
using Shared.Services;

namespace Squadtalk.Client.Services;

public class HttpMessagePageProvider(RestClient client, ILogger<HttpMessagePageProvider> logger)
    : IMessagePageProvider
{
    private readonly List<IChatMessage> _empty = [];
    
    public async Task<List<IChatMessage>> GetPageAsync(
        ChannelId channelId, MessageCursor cursor, CancellationToken cancellationToken)
    {
        var resource = cursor == default
            ? $"api/message/{channelId.Value}"
            : $"api/message/{channelId.Value}/{cursor.Value}";

        var request = new RestRequest(resource);

        try
        {
            var response = await client.GetAsync<List<MessageDto>>(request, cancellationToken);
            
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