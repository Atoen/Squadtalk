using System.Diagnostics;
using MessagePack;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Shared.Enums;
using Shared.Services;
using Shared.Signalr;
using Squadtalk.Client.Data;

namespace Squadtalk.Client.Services.SignalR;

internal sealed partial class SignalrService : IConnectionService, IAsyncDisposable
{
    private readonly ClientPersistantState _clientPersistantState;
    private readonly NotificationService _notificationService;
    private readonly ILogger<SignalrService> _logger;
    private readonly HubConnection _connection;

    private bool _handlersRegistered;
    private bool _connectionStared;

    public event Action<ConnectionStatus>? ConnectionStatusChanged;

    public ConnectionStatus ConnectionStatus { get; private set; } = ConnectionStatus.Connecting;
    public UserStatus UserStatus { get; private set; } = UserStatus.Unknown;

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

        TryGetPersistedData();

        if (!_handlersRegistered)
        {
            RegisterHandlers();
            _handlersRegistered = true;
        }

        try
        {
            await _connection.StartAsync();

            ConnectionStatus = ConnectionStatus.Connected;
            ConnectionStatusChanged?.Invoke(ConnectionStatus);

            var result = await InvokeAsync<UserStatus>(HubMethods.GetSelfStatus);
            if (result.IsSuccess)
            {
                UserStatus = result.Value;

                _logger.LogInformation("Connected with status: {Status}", UserStatus);
                UserStatusChanged?.Invoke();
            }
        }
        catch (Exception e)
        {
            ConnectionStatus = ConnectionStatus.Disconnected;
            ConnectionStatusChanged?.Invoke(ConnectionStatus);

            _logger.LogError(e, "Failed to connect to chat hub");
            _notificationService.FailedToConnectToChatHub();
        }
    }

    public async Task<TimeSpan> MeasureConnectionDelayAsync()
    {
        var start = Stopwatch.GetTimestamp();

        var result = await InvokeAsync<bool>("Ping");
        return result.IsSuccess ? Stopwatch.GetElapsedTime(start) : TimeSpan.Zero;
    }

    public Task OnPersisting() => throw new InvalidOperationException();

    private void TryGetPersistedData()
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
            ChannelsReceived?.Invoke(channels);
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
            UserStatus = UserStatus.Unknown;

            ConnectionStatusChanged?.Invoke(ConnectionStatus);
            UserStatusChanged?.Invoke();

            return Task.CompletedTask;
        };

        _connection.Reconnected += async _ =>
        {
            ConnectionStatus = ConnectionStatus.Connected;
            ConnectionStatusChanged?.Invoke(ConnectionStatus);

            var result = await InvokeAsync<UserStatus>(HubMethods.GetSelfStatus);
            if (result.IsSuccess)
            {
                UserStatus = result.Value;
            }

            UserStatusChanged?.Invoke();
        };

        _connection.Closed += _ =>
        {
            ConnectionStatus = ConnectionStatus.Disconnected;
            UserStatus = UserStatus.Unknown;

            ConnectionStatusChanged?.Invoke(ConnectionStatus);
            UserStatusChanged?.Invoke();

            return Task.CompletedTask;
        };
    }

    private async Task<SignalrResult<T>> InvokeAsync<T>(string methodName, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _connection.InvokeAsync<T>(methodName, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while invoking hub method {MethodName}", methodName);
            return SignalrResult<T>.Error;
        }
    }

    private async Task<SignalrResult<T>> InvokeAsync<T>(string methodName, object? arg, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _connection.InvokeAsync<T>(methodName, arg, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while invoking hub method {MethodName}", methodName);
            return SignalrResult<T>.Error;
        }
    }

    private async Task<SignalrResult<T>> InvokeAsync<T>(string methodName, object? arg1, object? arg2, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _connection.InvokeAsync<T>(methodName, arg1, arg2, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while invoking hub method {MethodName}", methodName);
            return SignalrResult<T>.Error;
        }
    }

    private async Task<SignalrResult> SendAsync(string methodName, CancellationToken cancellationToken = default)
    {
        try
        {
            await _connection.SendAsync(methodName, cancellationToken);
            return SignalrResult.Ok;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while invoking hub method {MethodName}", methodName);
            return SignalrResult.Error;
        }
    }

    private async Task<SignalrResult> SendAsync(string methodName, object? arg, CancellationToken cancellationToken = default)
    {
        try
        {
            await _connection.SendAsync(methodName, arg, cancellationToken);
            return SignalrResult.Ok;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while invoking hub method {MethodName}", methodName);
            return SignalrResult.Error;
        }
    }

    private async Task<SignalrResult> SendAsync(string methodName, object? arg1, object? arg2, CancellationToken cancellationToken = default)
    {
        try
        {
            await _connection.SendAsync(methodName, arg1, arg2, cancellationToken);
            return SignalrResult.Ok;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while invoking hub method {MethodName}", methodName);
            return SignalrResult.Error;
        }
    }

    public ValueTask DisposeAsync()
    {
        return _connection.DisposeAsync();
    }
}
