using Shared.Data;
using Shared.DTOs;
using Shared.Extensions;
using Shared.Services;

namespace Squadtalk.Services;

public sealed class ServersideSignalrService : ISignalrService
{
#pragma warning disable CS0067

    public event Func<MessageDto, Task>? MessageReceived;
    public event Func<UserDto, Task>? UserConnected;
    public event Func<UserDto, Task>? UserDisconnected;
    public event Func<IEnumerable<UserDto>, Task>? ConnectedUsersReceived;
    public event Func<string, Task>? ConnectionStatusChanged;
    public event Func<IEnumerable<ChannelDto>, Task>? TextChannelsReceived;
    public event Func<ChannelDto, Task>? AddedToTextChannel;
    
    public event Func<UserDto, CallOfferId, Task>? IncomingCall;
    public event Func<CallOfferId, Task>? CallAccepted;
    public event Func<CallOfferId, Task>? CallDeclined;
    public event Func<CallId, Task>? CallEnded;
    public event Func<string, Task>? CallFailed;
    public event Func<List<UserDto>, CallId, Task>? GetCallUsers;
    public event Func<VoicePacketDto, Task>? GetVoicePacket;

    public Task<CallOfferId?> StartVoiceCallAsync(UserId id)
    {
        throw new InvalidOperationException();
    }

    public Task EndCallAsync(CallId id)
    {
        throw new InvalidOperationException();
    }

    public Task AcceptCallAsync(CallOfferId id)
    {
        throw new InvalidOperationException();
    }

    public Task DeclineCallAsync(CallOfferId id)
    {
        throw new InvalidOperationException();
    }

    public Task StreamDataAsync(CallId callId, IAsyncEnumerable<byte[]> stream, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException();
    }

#pragma warning restore CS0067
    
    public string ConnectionStatus { get; private set; } = ISignalrService.Offline;

    public bool Connected => false;

    public Task ConnectAsync()
    {
        ConnectionStatus = ISignalrService.Connecting;
        return ConnectionStatusChanged.TryInvoke(ConnectionStatus);
    }

    public Task SendMessageAsync(string message, string channelId, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}