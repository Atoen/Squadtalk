using RestSharp;
using Shared.Data.TypedIds;

namespace Squadtalk.Client.Services;

public class CreateTextChannelRequestHandler
{
    private readonly RestClient _restClient;
    private readonly ILogger<CreateTextChannelRequestHandler> _logger;

    public CreateTextChannelRequestHandler(RestClient restClient, ILogger<CreateTextChannelRequestHandler> logger)
    {
        _restClient = restClient;
        _logger = logger;
    }
    
    public async Task<ChannelId?> CreateTextChannelAsync(IEnumerable<UserId> participants)
    {
        var request = new RestRequest("api/message/createChannel")
            .AddBody(participants);
        
        try
        {
            var createdChannelId = await _restClient.PostAsync<ChannelId>(request);
            return createdChannelId;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while sending create channel request");
            return null;
        }
    }
}
