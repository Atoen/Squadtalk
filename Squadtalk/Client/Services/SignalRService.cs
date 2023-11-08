using FluentResults;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Squadtalk.Shared;

namespace Squadtalk.Client.Services;

public delegate void ConnectedUsersReceivedHandler(IEnumerable<UserDto> users);

public delegate void UserDisconnectedHandler(UserDto user);
public delegate void UserConnectedHandler(UserDto user);

public delegate void ConnectionStatusChangedHandler(string status);

public sealed class SignalRService : IAsyncDisposable
{
    private readonly MessageService _messageService;
    private readonly HubConnection _connection;

    public string ConnectionStatus { get; private set; } = string.Empty;
    public event ConnectionStatusChangedHandler? StatusChanged;
    public event UserDisconnectedHandler? UserDisconnected;
    public event UserDisconnectedHandler? UserConnected;
    public event ConnectedUsersReceivedHandler? ConnectedUsersReceived;

    public const string Reconnecting = "Reconnecting";
    public const string Online = "Online";
    public const string Disconnected = "Disconnected";

    private bool _connected;
    
    public SignalRService(MessageService messageService, JwtService jwtService, IWebAssemblyHostEnvironment hostEnvironment)
    {
        _messageService = messageService;
        
        _connection = new HubConnectionBuilder()
            .WithUrl($"{hostEnvironment.BaseAddress}chat",
                options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult<string?>(jwtService.Token);
                })
            .WithAutomaticReconnect()
            .Build();
    }

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

        _connection.On<MessageDto>("ReceiveMessage",  async message =>
            await _messageService.HandleIncomingMessage(message));

        _connection.On<IEnumerable<UserDto>>("GetConnectedUsers", users =>
            ConnectedUsersReceived?.Invoke(users));

        _connection.On<UserDto>("UserDisconnected", user => UserDisconnected?.Invoke(user));
        _connection.On<UserDto>("UserConnected", user => UserConnected?.Invoke(user));
    }

    public async ValueTask DisposeAsync()
    {
        _connected = false;
        await _connection.DisposeAsync();
    }
}