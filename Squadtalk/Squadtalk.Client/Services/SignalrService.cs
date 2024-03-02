using MessagePack;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Shared.DTOs;
using Shared.Extensions;
using Shared.Services;
using Squadtalk.Client.SignalR;

namespace Squadtalk.Client.Services;

public sealed class SignalrService : ISignalrService
{
    private readonly ILogger<SignalrService> _logger;
    private readonly HubConnection _connection;

    private bool _handlersRegistered;

    public event Func<string, Task>? ConnectionStatusChanged;
    public event Func<IEnumerable<ChannelDto>, Task>? TextChannelsReceived;
    public event Func<ChannelDto, Task>? AddedToTextChannel;
    public event Func<UserDto, Task>? UserDisconnected;
    public event Func<UserDto, Task>? UserConnected;
    public event Func<IEnumerable<UserDto>, Task>? ConnectedUsersReceived;
    public event Func<MessageDto, Task>? MessageReceived;

    private bool _connectionStared;
    public bool Connected { get; private set; }

    public string ConnectionStatus { get; private set; } = ISignalrService.Offline;

    public SignalrService(NavigationManager navigationManager, ILogger<SignalrService> logger)
    {
        _logger = logger;

        var endpoint = navigationManager.ToAbsoluteUri("/chathub");
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
        
        if (!_handlersRegistered)
        {
            RegisterHandlers();
            _handlersRegistered = true;
        }

        try
        {
            ConnectionStatus = ISignalrService.Connecting;
            await ConnectionStatusChanged.TryInvoke(ConnectionStatus);
            await _connection.StartAsync();
            
            Connected = true;
            ConnectionStatus = ISignalrService.Online;
            await ConnectionStatusChanged.TryInvoke(ConnectionStatus);
            
            _logger.LogInformation("Successfully connected to chat hub");
        }
        catch
        {
            ConnectionStatus = ISignalrService.Disconnected;
            _logger.LogError("Failed to connect to chat hub");
        }
    }
    
    public Task SendMessageAsync(string message, string channelId, CancellationToken cancellationToken)
    {
        try
        {
            return _connection.SendAsync("SendMessage", message, channelId, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to send message");
            return Task.CompletedTask;
        }
    }

    public Task StartVoiceCallAsync(List<string> invitedIds)
    {
        try
        {
            return _connection.SendAsync("StartCall", invitedIds);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to start call");
            return Task.CompletedTask;
        }
    }

    private void RegisterHandlers()
    {
        _connection.Reconnecting += _ =>
        {
            ConnectionStatus = ISignalrService.Reconnecting;
            Connected = false;
            return ConnectionStatusChanged.TryInvoke(ConnectionStatus);
        };

        _connection.Reconnected += _ =>
        {
            ConnectionStatus = ISignalrService.Online;
            Connected = true;
            return ConnectionStatusChanged.TryInvoke(ConnectionStatus);
        };

        _connection.Closed += _ =>
        {
            ConnectionStatus = ISignalrService.Disconnected;
            Connected = false;
            return ConnectionStatusChanged.TryInvoke(ConnectionStatus);
        };

        _connection.On<MessageDto>("ReceiveMessage", message =>
            MessageReceived.TryInvoke(message));

        _connection.On<IEnumerable<UserDto>>("GetConnectedUsers", users =>
            ConnectedUsersReceived.TryInvoke(users));

        _connection.On<IEnumerable<ChannelDto>>("GetChannels", channels =>
            TextChannelsReceived.TryInvoke(channels));

        _connection.On<ChannelDto>("AddedToChannel", channel =>
            AddedToTextChannel.TryInvoke(channel));

        _connection.On<UserDto>("UserDisconnected", user =>
            UserDisconnected.TryInvoke(user));

        _connection.On<UserDto>("UserConnected", user =>
            UserConnected.TryInvoke(user));
        
        _connection.On<VoiceCallDto>("CallOfferIncoming",
            async call =>
            {
                _logger.LogInformation("Incoming voice call initiated by {User}", call.Initiator.Username);
                await _connection.InvokeAsync("AcceptCall", call.Id);
            });

        _connection.On<UserDto, string>("UserJoinedCall", (user, callId) =>
        {
            _logger.LogInformation("User {User} joined the call {Id}", user.Username, callId);
        });
    }

    public ValueTask DisposeAsync()
    {
        return _connection.DisposeAsync();
    }
}