using FluentResults;
using Microsoft.AspNetCore.SignalR.Client;
using Squadtalk.Shared;

namespace Squadtalk.Client.Services;

public sealed class SignalRService : IAsyncDisposable
{
    private readonly MessageService _messageService;
    private readonly HubConnection _connection;

    public string ConnectionStatus { get; private set; } = string.Empty;
    public Action? StatusChanged { get; set; }

    public const string Reconnecting = "Reconnecting";
    public const string Online = "Online";
    public const string Disconnected = "Disconnected";
    
    public SignalRService(MessageService messageService, JwtService jwtService)
    {
        _messageService = messageService;

        _connection = new HubConnectionBuilder()
            .WithUrl("https://squadtalk.net/chat",
                options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult<string?>(jwtService.Token);
                })
            .WithAutomaticReconnect()
            .Build();
    }

    public async Task<Result> ConnectAsync()
    {
        RegisterHandlers();

        try
        {
            await _connection.StartAsync();
            ConnectionStatus = Online;
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }

        return Result.Ok();
    }

    public async Task<Result> SendMessageAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            await _connection.InvokeAsync("SendMessage", message, cancellationToken);
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
            StatusChanged?.Invoke();
            return Task.CompletedTask;
        };

        _connection.Reconnected += _ =>
        {
            ConnectionStatus = Online;
            StatusChanged?.Invoke();
            return Task.CompletedTask;
        };

        _connection.Closed += _ =>
        {
            ConnectionStatus = Disconnected;
            StatusChanged?.Invoke();
            return Task.CompletedTask;
        };

        _connection.On<MessageDto>("ReceiveMessage", async message =>
        {
            await _messageService.HandleIncomingMessage(message);
        });
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}