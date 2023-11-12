using FluentResults;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Squadtalk.Shared;

namespace Squadtalk.Client.Services;

public delegate void ConnectedUsersReceivedHandler(IEnumerable<UserDto> users);

public delegate void UserDisconnectedHandler(UserDto user);

public delegate void UserConnectedHandler(UserDto user);

public delegate void ConnectionStatusChangedHandler(string status);

public delegate void ChannelsReceivedHandler(IEnumerable<ChannelDto> channel);

public delegate void AddedToChannelHandler(ChannelDto channel);

public sealed class SignalRService : IAsyncDisposable
{
    public const string Reconnecting = "Reconnecting";
    public const string Online = "Online";
    public const string Disconnected = "Disconnected";
    private readonly HubConnection _connection;
    private readonly MessageService _messageService;

    private bool _connected;

    public SignalRService(MessageService messageService, JwtService jwtService,
        IWebAssemblyHostEnvironment hostEnvironment)
    {
        _messageService = messageService;

        _connection = new HubConnectionBuilder()
            .WithUrl($"{hostEnvironment.BaseAddress}chat",
                options => { options.AccessTokenProvider = () => Task.FromResult<string?>(jwtService.Token); })
            .WithAutomaticReconnect()
            .Build();
    }

    public string ConnectionStatus { get; private set; } = string.Empty;

    public async ValueTask DisposeAsync()
    {
        _connected = false;
        await _connection.DisposeAsync();
    }

    public static event ConnectionStatusChangedHandler? StatusChanged;
    public static event UserDisconnectedHandler? UserDisconnected;
    public static event UserConnectedHandler? UserConnected;
    public static event ConnectedUsersReceivedHandler? ConnectedUsersReceived;
    public static event ChannelsReceivedHandler? ChannelsReceived;
    public static event AddedToChannelHandler? AddedToChannel;

    public async Task<Result> ConnectAsync()
    {
        if (_connected) return Result.Ok();

        RegisterHandlers();

        try
        {
            await _connection.StartAsync();
            ConnectionStatus = Online;
            StatusChanged?.Invoke(ConnectionStatus);
            _connected = true;
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }

        return Result.Ok();
    }

    public async Task<Result> SendMessageAsync(string message, Guid channelId, CancellationToken cancellationToken)
    {
        try
        {
            await _connection.InvokeAsync("SendMessage", message, channelId, cancellationToken);
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }

        return Result.Ok();
    }

    private void RegisterHandlers()
    {
        _connection.Reconnecting += _ =>
        {
            ConnectionStatus = Reconnecting;
            StatusChanged?.Invoke(ConnectionStatus);
            return Task.CompletedTask;
        };

        _connection.Reconnected += _ =>
        {
            ConnectionStatus = Online;
            StatusChanged?.Invoke(ConnectionStatus);
            return Task.CompletedTask;
        };

        _connection.Closed += _ =>
        {
            ConnectionStatus = Disconnected;
            StatusChanged?.Invoke(ConnectionStatus);
            return Task.CompletedTask;
        };

        _connection.On<MessageDto>("ReceiveMessage", async message =>
            await _messageService.HandleIncomingMessage(message));

        _connection.On<IEnumerable<UserDto>>("GetConnectedUsers", users =>
            ConnectedUsersReceived?.Invoke(users));

        _connection.On<IEnumerable<ChannelDto>>("GetChannels", channels =>
            ChannelsReceived?.Invoke(channels));

        _connection.On<ChannelDto>("AddedToChannel", channel =>
            AddedToChannel?.Invoke(channel));

        _connection.On<UserDto>("UserDisconnected", user => UserDisconnected?.Invoke(user));
        _connection.On<UserDto>("UserConnected", user => UserConnected?.Invoke(user));
    }
}