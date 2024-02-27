using MessagePack;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Shared.Services;
using Squadtalk.Client.SignalR;

namespace Squadtalk.Client.Services;

public sealed class VoiceChatService : IVoiceChatService
{
    private readonly ILogger<VoiceChatService> _logger;
    private readonly HubConnection _connection;
    
    private bool _connectionStared;
    
    public bool Connected { get; private set; }

    public VoiceChatService(NavigationManager navigationManager, ILogger<VoiceChatService> logger)
    {
        _logger = logger;
        
        var endpoint = navigationManager.ToAbsoluteUri("/voicehub");
        _logger.LogInformation("Building hub connection to {Endpoint}", endpoint);

        _connection = new HubConnectionBuilder()
            .WithUrl(endpoint, options =>
            {
                options.HttpMessageHandlerFactory = innerHandler =>
                    new IncludeRequestCredentialsMessageHandler { InnerHandler = innerHandler };
            })
            .WithAutomaticReconnect()
            .WithStatefulReconnect()
            .AddMessagePackProtocol(options =>
            {
                options.SerializerOptions = MessagePackSerializerOptions.Standard
                    .WithCompression(MessagePackCompression.Lz4BlockArray)
                    .WithSecurity(MessagePackSecurity.UntrustedData)
                    .WithCompressionMinLength(256);
            })
            .Build();
    }
    
    public async Task ConnectAsync()
    {
        if (_connectionStared) return;
        _connectionStared = true;

        try
        {
            await _connection.StartAsync();
            Connected = true;
            _logger.LogInformation("Successfully connected to chat hub");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to connect to chat hub");
        }
    }

    public async Task StartStreamAsync<T>(IAsyncEnumerable<T> stream, CancellationToken cancellationToken)
    {
        await _connection.SendAsync("StartStream", stream, cancellationToken: cancellationToken);
    }

    public IAsyncEnumerable<int> GetStream()
    {
        return _connection.StreamAsync<int>("Counter");
    }
    
    public ValueTask DisposeAsync()
    {
        return _connection.DisposeAsync();
    }
}