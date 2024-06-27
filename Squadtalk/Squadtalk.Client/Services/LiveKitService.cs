using RestSharp;
using Shared.DTOs;
using Shared.Services;

namespace Squadtalk.Client.Services;

public class LiveKitService : ILiveKitService
{
    private readonly RestClient _client;
    private readonly ILogger<LiveKitService> _logger;

    public LiveKitService(RestClient client, ILogger<LiveKitService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<RoomTokenDto?> CreateRoomTokenAsync(string username, string roomName)
    {
        var request = new RestRequest("/api/LiveKit/CreateRoomToken", Method.Post)
            .AddBody(new CreateRoomRequestDto
            {
                Username = username,
                RoomName = roomName
            });

        try
        {
            return await _client.PostAsync<RoomTokenDto>(request);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while creating room token");
            return null;
        }
    }
}
