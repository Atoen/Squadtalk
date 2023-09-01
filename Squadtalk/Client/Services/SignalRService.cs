using FluentResults;
using Microsoft.AspNetCore.SignalR.Client;
using Squadtalk.Shared;

namespace Squadtalk.Client.Services;

public sealed class SignalRService : IAsyncDisposable
{
    private readonly MessageService _messageService;
    private readonly JwtService _jwtService;
    private readonly HubConnection _connection;

    public string ConnectionStatus { get; private set; } = string.Empty;

    private const string Reconnecting = "Reconnecting";
    private const string Online = "Reconnecting";
    private const string Disconnected = "Reconnecting";
    
    public SignalRService(MessageService messageService, JwtService jwtService)
    {
        _messageService = messageService;
        _jwtService = jwtService;

        _connection = new HubConnectionBuilder()
            .WithUrl("http://squadtalk.ddns.net/chat",
                options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult<string?>(_jwtService.Token);
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
            return Task.CompletedTask;
        };

        _connection.Reconnected += _ =>
        {
            ConnectionStatus = Online;
            return Task.CompletedTask;
        };

        _connection.Closed += _ =>
        {
            ConnectionStatus = Disconnected;
            return Task.CompletedTask;
        };

        _connection.On<MessageDto>("ReceiveMessage", message =>
        {
            _messageService.HandleIncomingMessage(message);
        });
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}