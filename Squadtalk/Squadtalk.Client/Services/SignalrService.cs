using System.Diagnostics;
using MessagePack;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Shared.Data.TypedIds;
using Shared.DTOs;
using Shared.Extensions;
using Shared.Services;
using Squadtalk.Client.SignalR;

namespace Squadtalk.Client.Services;

public sealed class SignalrService : ISignalrService, IAsyncDisposable
{
    private readonly ILogger<SignalrService> _logger;
    private readonly HubConnection _connection;

    private bool _handlersRegistered;

    public event Func<string, Task>? ConnectionStatusChanged;
    public event Func<IEnumerable<ChannelDto>, Task>? ChannelsReceived;
    public event Func<ChannelDto, Task>? AddedToChannel;
    public event Func<UserDto, Task>? UserDisconnected;
    public event Func<UserDto, Task>? UserConnected;
    public event Func<IEnumerable<UserDto>, Task>? ConnectedUsersReceived;
    public event Func<MessageDto, Task>? MessageReceived;
    public event Func<ChannelId, string?, Task>? ChannelNameChanged;

    public event Func<ChannelId, UserId, Task>? IncomingCall;
    public event Func<ChannelId, UserDto, Task>? CallAccepted;
    public event Func<UserDto, ChannelId, Task>? CallDeclined;
    public event Func<ChannelId, Task>? CallEnded;
    public event Func<string, Task>? CallFailed;

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
        catch (Exception e)
        {
            ConnectionStatus = ISignalrService.Disconnected;
            _logger.LogError(e, "Failed to connect to chat hub");
        }
    }

    public async Task<TimeSpan> MeasureClientDelayAsync()
    {
        var start = Stopwatch.GetTimestamp();

        await _connection.InvokeAsync<TimeSpan>("Ping");
        return Stopwatch.GetElapsedTime(start);
    }

    Task ISignalrTextService.SendMessageAsync(string message, ChannelId channelId, CancellationToken cancellationToken)
    {
        return _connection.SendAsync("SendMessage", message, channelId, cancellationToken);
    }

    Task<RoomTokenDto?> ISignalrVoiceService.StartVoiceCallAsync(ChannelId id)
    {
        return _connection.InvokeAsync<RoomTokenDto?>("StartCall", id);
    }

    Task<RoomTokenDto?> ISignalrVoiceService.AcceptCallAsync(ChannelId id)
    {
        return _connection.InvokeAsync<RoomTokenDto?>("AcceptCall", id);
    }

    Task ISignalrVoiceService.DeclineCallAsync(ChannelId id)
    {
        return _connection.SendAsync("DeclineCall", id);
    }

    Task<bool> ISignalrVoiceService.ChannelHasActiveCall(ChannelId id)
    {
        return _connection.InvokeAsync<bool>("ChannelHasActiveCall", id);
    }

    Task<bool> ISignalrTextService.ChangeChannelNameAsync(string? newName, ChannelId channelId)
    {
        return _connection.InvokeAsync<bool>("ChangeGroupName", newName, channelId);
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
            ChannelsReceived.TryInvoke(channels));

        _connection.On<ChannelDto>("AddedToChannel", channel =>
            AddedToChannel.TryInvoke(channel));

        _connection.On<UserDto>("UserDisconnected", user =>
            UserDisconnected.TryInvoke(user));

        _connection.On<UserDto>("UserConnected", user =>
            UserConnected.TryInvoke(user));

        _connection.On<ChannelId, string>("ChannelNameChanged", (channelId, name) =>
            ChannelNameChanged.TryInvoke(channelId, name));

        _connection.On<ChannelId, UserId>("IncomingCall", (channelId, initiatorId) =>
            IncomingCall.TryInvoke(channelId, initiatorId));

        _connection.On<ChannelId, UserDto>("CallAccepted", (channelId, accepting) =>
            CallAccepted.TryInvoke(channelId, accepting));

        _connection.On<UserDto, ChannelId>("CallDeclined", (user, channelId) =>
            CallDeclined.TryInvoke(user, channelId));

        _connection.On<ChannelId>("CallEnded", channelId =>
            CallEnded.TryInvoke(channelId));

        _connection.On<string>("CallFailed", reason =>
            CallFailed.TryInvoke(reason));
    }

    public ValueTask DisposeAsync()
    {
        return _connection.DisposeAsync();
    }
}
