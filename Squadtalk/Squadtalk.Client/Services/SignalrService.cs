using System.Runtime.CompilerServices;
using MessagePack;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Shared.Data;
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
    
    public event Func<UserDto, CallOfferId, Task>? IncomingCall;
    public event Func<CallOfferId, Task>? CallAccepted;
    public event Func<CallOfferId, Task>? CallDeclined;
    public event Func<CallId, Task>? CallEnded;
    public event Func<string, Task>? CallFailed;
    public event Func<List<UserDto>, CallId, Task>? GetCallUsers;
    public event Func<VoicePacketDto, Task>? GetVoicePacket;

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

    Task ISignalrTextService.SendMessageAsync(string message, string channelId, CancellationToken cancellationToken)
    {
        return SendAsync("SendMessage", message, channelId, cancellationToken);
    }

    Task<CallOfferId?> ISignalrVoiceService.StartVoiceCallAsync(UserId id)
    {
        return InvokeAsync<CallOfferId?, UserId>("StartCall", id);
    }
    
    Task ISignalrVoiceService.EndCallAsync(CallId id)
    {
        return SendAsync("EndCall", id);
    }

    Task ISignalrVoiceService.AcceptCallAsync(CallOfferId id)
    {
        return SendAsync("AcceptCall", id);
    }

    Task ISignalrVoiceService.DeclineCallAsync(CallOfferId id)
    {
        return SendAsync("DeclineCall", id);
    }
    
    Task ISignalrVoiceService.StreamDataAsync(CallId callId, IAsyncEnumerable<byte[]> stream, CancellationToken cancellationToken)
    {
        return SendAsync("StartStream", callId, stream, cancellationToken);
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
        
        
        _connection.On<UserDto, CallOfferId>("IncomingCall", (caller, offerId) =>
            IncomingCall.TryInvoke(caller, offerId));

        _connection.On<CallOfferId>("CallAccepted", offerId =>
            CallAccepted.TryInvoke(offerId));
        
        _connection.On<CallOfferId>("CallDeclined", offerId =>
            CallDeclined.TryInvoke(offerId));
        
        _connection.On<CallId>("CallEnded", callId =>
            CallEnded.TryInvoke(callId));
        
        _connection.On<string>("CallFailed", reason =>
            CallFailed.TryInvoke(reason));

        _connection.On<List<UserDto>, CallId>("GetCallUsers", (users, callId) =>
            GetCallUsers.TryInvoke(users, callId));

        _connection.On<VoicePacketDto>("GetVoicePacket", packet => 
            GetVoicePacket.TryInvoke(packet));
    }
    
    private async Task<TResult?> InvokeAsync<TResult, TArg>(string methodName, TArg arg,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string? callerName = null)
    {
        try
        {
            return await _connection.InvokeCoreAsync<TResult>(methodName, [arg], cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e,"{CallerName}: Error while dispatching message", callerName);
            return default;
        }
    }
    
    private Task SendAsync<T>(string methodName, T arg, CancellationToken cancellationToken = default,
        [CallerMemberName] string? callerName = null)
    {
        try
        {
            return _connection.SendAsync(methodName, arg, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e,"{CallerName}: Error while dispatching message", callerName);
            return Task.CompletedTask;
        }
    }
    
    private Task SendAsync<T1, T2>(string methodName, T1 arg1, T2 arg2, CancellationToken cancellationToken = default, 
        [CallerMemberName] string? callerName = null)
    {
        try
        {
            return _connection.SendAsync(methodName, arg1, arg2, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e,"{CallerName}: Error while dispatching message", callerName);
            return Task.CompletedTask;
        }
    }

    public ValueTask DisposeAsync()
    {
        return _connection.DisposeAsync();
    }
}