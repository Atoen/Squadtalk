using System.Diagnostics;
using MessagePack;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Extensions;
using Shared.Services;

namespace Squadtalk.Client.Services.SignalR;

internal sealed partial class SignalrService : IConnectionService, IAsyncDisposable
{
    private readonly ClientPersistantState _clientPersistantState;
    private readonly NotificationService _notificationService;
    private readonly ILogger<SignalrService> _logger;
    private readonly HubConnection _connection;

    private bool _handlersRegistered;

    public event Func<ConnectionStatus, Task>? ConnectionStatusChanged;

    public event Func<ChannelDto, Task>? AddedToChannel;
    public event Func<IEnumerable<ChannelDto>, Task>? ChannelsReceived;
    public event Func<ChannelId, string?, Task>? ChannelNameChanged;

    private bool _connectionStared;

    public ConnectionStatus ConnectionStatus { get; private set; } = ConnectionStatus.Connecting;

    public SignalrService(
        NavigationManager navigationManager,
        ClientPersistantState clientPersistantState,
        NotificationService notificationService,
        ILogger<SignalrService> logger)
    {
        _clientPersistantState = clientPersistantState;
        _notificationService = notificationService;
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

        await TryGetPersistedData();

        if (!_handlersRegistered)
        {
            RegisterHandlers();
            _handlersRegistered = true;
        }

        try
        {
            await _connection.StartAsync();

            ConnectionStatus = ConnectionStatus.Connected;
            await ConnectionStatusChanged.TryInvoke(ConnectionStatus);

        }
        catch (Exception e)
        {
            ConnectionStatus = ConnectionStatus.Disconnected;
            _logger.LogError(e, "Failed to connect to chat hub");
        }
    }

    public async Task<TimeSpan> MeasureConnectionDelayAsync()
    {
        var start = Stopwatch.GetTimestamp();

        await _connection.InvokeAsync<bool>("Ping");
        return Stopwatch.GetElapsedTime(start);
    }

    public Task OnPersisting() => throw new InvalidOperationException();

    public Task<bool> ChangeChannelNameAsync(string? newName, ChannelId channelId)
    {
        return _connection.InvokeAsync<bool>("ChangeGroupName", newName, channelId);
    }

    private async Task TryGetPersistedData()
    {
        if (_clientPersistantState.TryReadFriends(out var friends))
        {
            _logger.LogInformation("Retrieved persisted friends");
            FriendListReceived?.Invoke(friends);
        }

        if (_clientPersistantState.TryReadFriendRequests(out var friendRequests))
        {
            _logger.LogInformation("Retrieved persisted friend requests");
            FriendRequestsReceived?.Invoke(friendRequests);
        }

        if (_clientPersistantState.TryReadChannels(out var channels))
        {
            _logger.LogInformation("Retrieved persisted channels");
            await ChannelsReceived.TryInvoke(channels);
        }
    }

    private void RegisterHandlers()
    {
        RegisterBaseHandlers();
        RegisterFriendHandlers();
        RegisterTextHandlers();
        RegisterRTCHandlers();
    }

    private void RegisterBaseHandlers()
    {
        _connection.Reconnecting += _ =>
        {
            ConnectionStatus = ConnectionStatus.Reconnecting;
            return ConnectionStatusChanged.TryInvoke(ConnectionStatus);
        };

        _connection.Reconnected += _ =>
        {
            ConnectionStatus = ConnectionStatus.Connected;
            return ConnectionStatusChanged.TryInvoke(ConnectionStatus);
        };

        _connection.Closed += _ =>
        {
            ConnectionStatus = ConnectionStatus.Disconnected;
            return ConnectionStatusChanged.TryInvoke(ConnectionStatus);
        };
    }

        // _connection.On<IEnumerable<UserDto>>("GetConnectedUsers", users =>
        //     ConnectedUsersReceived.TryInvoke(users, false));
        //
        // _connection.On<IEnumerable<ChannelDto>>("GetChannels", channels =>
        //     ChannelsReceived.TryInvoke(channels));
        //
        // _connection.On<ChannelDto>("AddedToChannel", channel =>
        //     AddedToChannel.TryInvoke(channel));
        //
        // _connection.On<ChannelId, string>("ChannelNameChanged", (channelId, name) =>
        //     ChannelNameChanged.TryInvoke(channelId, name));
    // }

    private async Task<T?> InvokeAsync<T>(string methodName, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _connection.InvokeAsync<T>(methodName, cancellationToken);
        }
        catch
        {
            _notificationService.ShowUnableToConnectNotification();
            return default;
        }
    }

    private async Task<T?> InvokeAsync<T>(string methodName, object? arg, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _connection.InvokeAsync<T>(methodName, arg, cancellationToken);
        }
        catch
        {
            _notificationService.ShowUnableToConnectNotification();
            return default;
        }
    }

    private async Task<T?> InvokeAsync<T>(string methodName, object? arg1, object? arg2, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _connection.InvokeAsync<T>(methodName, arg1, arg2, cancellationToken);
        }
        catch
        {
            _notificationService.ShowUnableToConnectNotification();
            return default;
        }
    }

    public ValueTask DisposeAsync()
    {
        return _connection.DisposeAsync();
    }
}
