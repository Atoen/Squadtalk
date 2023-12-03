using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Squadtalk.Client.SignalR;

namespace Squadtalk.Client.Services;

public class SignalRService
{
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<SignalRService> _logger;
    private readonly HubConnection _connection;
    
    public SignalRService(NavigationManager navigationManager, ILogger<SignalRService> logger)
    {
        _navigationManager = navigationManager;
        _logger = logger;

        var endpoint = _navigationManager.ToAbsoluteUri("/chathub");
        _logger.LogInformation("Attempting to connect to {Endpoint}", endpoint);

        _connection = new HubConnectionBuilder()
            .WithUrl(endpoint, options =>
            {
                options.HttpMessageHandlerFactory = innerHandler =>
                    new IncludeRequestCredentialsMessageHandler { InnerHandler = innerHandler };
            })
            .WithAutomaticReconnect()
            .WithStatefulReconnect()
            .Build();
    }
}