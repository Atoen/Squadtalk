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

internal sealed partial class SignalrService : IConnectionService, IAsyncDisposable
{
    private readonly ClientPersistantState _clientPersistantState;
    private readonly ILogger<SignalrService> _logger;
    private readonly HubConnection _connection;

    private bool _handlersRegistered;

    public event Func<string, Task>? ConnectionStatusChanged;
    public event Func<IEnumerable<ChannelDto>, Task>? ChannelsReceived;
    public event Func<ChannelDto, Task>? AddedToChannel;
    public event Func<UserDto, Task>? UserDisconnected;
    public event Func<UserDto, Task>? UserConnected;
    public event Func<IEnumerable<UserDto>, bool, Task>? ConnectedUsersReceived;
    public event Func<ChannelId, string?, Task>? ChannelNameChanged;

    private bool _connectionStared;
    public bool Connected { get; private set; }

    public string ConnectionStatus { get; private set; } = "Offline";

    public SignalrService(
        NavigationManager navigationManager,
        ClientPersistantState clientPersistantState,
        ILogger<SignalrService> logger)
    {
        _clientPersistantState = clientPersistantState;
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
            // ConnectionStatus = ISignalrService.Connecting;
            ConnectionStatus = "Connecting";
            await ConnectionStatusChanged.TryInvoke(ConnectionStatus);
            await _connection.StartAsync();

            Connected = true;
            ConnectionStatus = "Online";
            // ConnectionStatus = ISignalrService.Online;
            await ConnectionStatusChanged.TryInvoke(ConnectionStatus);

        }
        catch (Exception e)
        {
            ConnectionStatus = "Disconnected";
            // ConnectionStatus = ISignalrService.Disconnected;
            _logger.LogError(e, "Failed to connect to chat hub");
        }

        await UpdatePersistedDataAsync();
    }

    public async Task<TimeSpan> MeasureConnectionDelayAsync()
    {
        var start = Stopwatch.GetTimestamp();

        await _connection.InvokeAsync("Ping");
        return Stopwatch.GetElapsedTime(start);
    }

    public Task OnPersisting() => throw new NotImplementedException();

    public Task<bool> ChangeChannelNameAsync(string? newName, ChannelId channelId)
    {
        return _connection.InvokeAsync<bool>("ChangeGroupName", newName, channelId);
    }

    private async Task TryGetPersistedData()
    {
        if (_clientPersistantState.TryReadUsers(out var users))
        {
            _logger.LogInformation("Retrieved persisted users");
            await ConnectedUsersReceived.TryInvoke(users, true);
        }

        if (_clientPersistantState.TryReadChannels(out var channels))
        {
            _logger.LogInformation("Retrieved persisted channels");
            await ChannelsReceived.TryInvoke(channels);
        }

        // if (_persistentComponentState.TryTakeFromJson<List<UserDto>>(IPersistState.Users, out var persistedUsers) && persistedUsers is not null)
        // {
        //     _logger.LogInformation("Retrieved persisted users");
        //     await ConnectedUsersReceived.TryInvoke(persistedUsers, true);
        // }
        //
        // if (_persistentComponentState.TryTakeFromJson<List<ChannelDto>>(IPersistState.Channels, out var persistedChannels) && persistedChannels is not null)
        // {
        //     _logger.LogInformation("Retrieved persisted channels");
        //     await ChannelsReceived.TryInvoke(persistedChannels);
        // }
    }

    private async Task UpdatePersistedDataAsync()
    {
        var (users, channels) = await _connection.InvokeAsync<(IEnumerable<UserDto>, IEnumerable<ChannelDto>)>("GetUsersAndChannels");

        await ConnectedUsersReceived.TryInvoke(users, false);
        await ChannelsReceived.TryInvoke(channels);
    }

    private void RegisterHandlers()
    {
        RegisterBaseHandlers();
        RegisterTextHandlers();
        RegisterRTCHandlers();
    }

    private void RegisterBaseHandlers()
    {
        _connection.Reconnecting += _ =>
        {
            ConnectionStatus = "Reconnecting";
            Connected = false;
            return ConnectionStatusChanged.TryInvoke(ConnectionStatus);
        };

        _connection.Reconnected += _ =>
        {
            ConnectionStatus = "Online";
            Connected = true;
            return ConnectionStatusChanged.TryInvoke(ConnectionStatus);
        };

        _connection.Closed += _ =>
        {
            ConnectionStatus = "Disconnected";
            Connected = false;
            return ConnectionStatusChanged.TryInvoke(ConnectionStatus);
        };

        _connection.On<IEnumerable<UserDto>>("GetConnectedUsers", users =>
            ConnectedUsersReceived.TryInvoke(users, false));

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
    }

    public ValueTask DisposeAsync()
    {
        return _connection.DisposeAsync();
    }
}
