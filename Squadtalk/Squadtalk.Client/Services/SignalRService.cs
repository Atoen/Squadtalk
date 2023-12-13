using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Shared.DTOs;
using Shared.Services;
using Squadtalk.Client.SignalR;

namespace Squadtalk.Client.Services;

public sealed class SignalRService : ISignalrService
{
    private readonly ILogger<SignalRService> _logger;

    private readonly HubConnection _connection;

    private bool _connected;
    private bool _handlersRegistered;
    
    private string _connectionStatus = ISignalrService.Connecting;

    public event Func<string, Task>? ConnectionStatusChanged;
    public event Func<UserDto, Task>? UserDisconnected;
    public event Func<UserDto, Task>? UserConnected;
    public event Func<IEnumerable<UserDto>, Task>? ConnectedUsersReceived;
    public event Func<MessageDto, Task>? MessageReceived;

    public event ChannelsReceivedHandler? ChannelsReceived;
    public event AddedToChannelHandler? AddedToChannel;
    
    public string ConnectionStatus
    {
        get => _connectionStatus;
        private set
        {
            var previousStatus = _connectionStatus;
            _connectionStatus = value;

            if (previousStatus != _connectionStatus)
            {
                ConnectionStatusChanged?.Invoke(_connectionStatus);
            }
        }
    }
    
    public SignalRService(NavigationManager navigationManager, ILogger<SignalRService> logger)
    {
        _logger = logger;

        var endpoint = navigationManager.ToAbsoluteUri("/chathub");
        _logger.LogInformation("Building hub connection to {Endpoint}", endpoint);

        _connection = new HubConnectionBuilder()
            .WithUrl(endpoint, options =>
            {
                options.HttpMessageHandlerFactory = innerHandler =>
                    new IncludeRequestCredentialsMessageHandler { InnerHandler = innerHandler };
                
                // options.Cookies
            })
            .WithAutomaticReconnect()
            .WithStatefulReconnect()
            .Build();
    }

    public async Task ConnectAsync()
    {
        if (_connected) return;

        if (!_handlersRegistered)
        {
            RegisterHandlers();
            _handlersRegistered = true;
        }

        try
        {
            await _connection.StartAsync();
            _connected = true;
            
            ConnectionStatus = ISignalrService.Online;
            
            _logger.LogInformation("Successfully connected to chat hub");
        }
        catch
        {
            ConnectionStatus = ISignalrService.Disconnected;
            _logger.LogError("Failed to connect to chat hub");
        }
    }
    
    public async Task SendMessageAsync(string message, string channelId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _connection.InvokeAsync("SendMessage", message, channelId, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to send message");
        }
        
    }
    
    private void RegisterHandlers()
    {
        _connection.Reconnecting += _ =>
        {
            ConnectionStatus = ISignalrService.Reconnecting;
            ConnectionStatusChanged?.Invoke(ConnectionStatus);
            return Task.CompletedTask;
        };

        _connection.Reconnected += _ =>
        {
            ConnectionStatus = ISignalrService.Online;
            ConnectionStatusChanged?.Invoke(ConnectionStatus);
            return Task.CompletedTask;
        };

        _connection.Closed += _ =>
        {
            ConnectionStatus = ISignalrService.Disconnected;
            ConnectionStatusChanged?.Invoke(ConnectionStatus);
            return Task.CompletedTask;
        };

        _connection.On<MessageDto>("ReceiveMessage", message =>
            MessageReceived is not null
                ? MessageReceived.Invoke(message)
                : Task.CompletedTask);

        _connection.On<IEnumerable<UserDto>>("GetConnectedUsers", users =>
            ConnectedUsersReceived is not null
                ? ConnectedUsersReceived.Invoke(users)
                : Task.CompletedTask);

        _connection.On<IEnumerable<ChannelDto>>("GetChannels", channels =>
            ChannelsReceived is not null
                ? ChannelsReceived.Invoke(channels)
                : Task.CompletedTask);

        _connection.On<ChannelDto>("AddedToChannel", channel =>
            AddedToChannel is not null
                ? AddedToChannel.Invoke(channel)
                : Task.CompletedTask);

        _connection.On<UserDto>("UserDisconnected", user =>
            UserDisconnected is not null
                ? UserDisconnected.Invoke(user)
                : Task.CompletedTask);
        
        _connection.On<UserDto>("UserConnected", user =>
            UserConnected is not null
                ? UserConnected.Invoke(user)
                : Task.CompletedTask);
    }

    public ValueTask DisposeAsync()
    {
        return _connection.DisposeAsync();
    }
}