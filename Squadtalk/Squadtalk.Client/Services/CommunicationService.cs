using Shared.Data;
using Shared.DTOs;
using Shared.Services;

namespace Squadtalk.Client.Services;

public class CommunicationService : ICommunicationService
{
    private readonly ISignalrService _signalrService;

    public CommunicationService(ISignalrService signalrService)
    {
        _signalrService = signalrService;
    }

    public string ConnectionStatus => _signalrService.ConnectionStatus;
    public bool Connected => _signalrService.Connected;

    public Task ConnectAsync()
    {
        return _signalrService.ConnectAsync();
    }

    public Task SendMessageAsync(string content, ChannelId channelId, CancellationToken cancellationToken)
    {
        return _signalrService.SendMessageAsync(content, channelId, cancellationToken);
    }

    public Task<CallOfferId?> StartVoiceCallAsync(UserId id)
    {
        return _signalrService.StartVoiceCallAsync(id);
    }

    public Task EndCallAsync(CallId id)
    {
        return _signalrService.EndCallAsync(id);
    }

    public Task AcceptCallAsync(CallOfferId id)
    {
        return _signalrService.AcceptCallAsync(id);
    }

    public Task DeclineCallAsync(CallOfferId id)
    {
        return _signalrService.DeclineCallAsync(id);
    }

    public Task StreamDataAsync(CallId callId, IAsyncEnumerable<byte[]> stream, CancellationToken cancellationToken)
    {
        return _signalrService.StreamDataAsync(callId, stream, cancellationToken);
    }

    public event Func<IChatMessage, Task>? MessageReceived
    {
        add => _signalrService.MessageReceived += value;
        remove => _signalrService.MessageReceived -= value;
    }

    public event Func<IChatUser, Task>? UserConnected
    {
        add => _signalrService.UserConnected += value;
        remove => _signalrService.UserConnected -= value;
    }

    public event Func<IChatUser, Task>? UserDisconnected
    {
        add => _signalrService.UserDisconnected += value;
        remove => _signalrService.UserDisconnected -= value;
    }

    public event Func<IEnumerable<IChatUser>, Task>? ConnectedUsersReceived
    {
        add => _signalrService.ConnectedUsersReceived += value;
        remove => _signalrService.ConnectedUsersReceived -= value;
    }

    public event Func<string, Task>? ConnectionStatusChanged
    {
        add => _signalrService.ConnectionStatusChanged += value;
        remove => _signalrService.ConnectionStatusChanged -= value;
    }

    public event Func<IEnumerable<IChatChannel>, Task>? TextChannelsReceived
    {
        add => _signalrService.TextChannelsReceived += value;
        remove => _signalrService.TextChannelsReceived -= value;
    }
    
    public event Func<IChatChannel, Task>? AddedToTextChannel
    {
        add => _signalrService.AddedToTextChannel += value;
        remove => _signalrService.AddedToTextChannel -= value;
    }
    
    public event Func<IChatUser, CallOfferId, Task>? IncomingCall
    {
        add => _signalrService.IncomingCall += value;
        remove => _signalrService.IncomingCall -= value;
    }

    public event Func<CallOfferId, Task>? CallAccepted
    {
        add => _signalrService.CallAccepted += value;
        remove => _signalrService.CallAccepted -= value;
    }

    public event Func<CallOfferId, Task>? CallDeclined
    {
        add => _signalrService.CallDeclined += value;
        remove => _signalrService.CallDeclined -= value;
    }

    public event Func<CallId, Task>? CallEnded
    {
        add => _signalrService.CallEnded += value;
        remove => _signalrService.CallEnded -= value;
    }

    public event Func<string, Task>? CallFailed
    {
        add => _signalrService.CallFailed += value;
        remove => _signalrService.CallFailed -= value;
    }

    public event Func<IEnumerable<IChatUser>, CallId, Task>? GetCallUsers
    {
        add => _signalrService.GetCallUsers += value;
        remove => _signalrService.GetCallUsers -= value;
    }

    public event Func<VoicePacketDto, Task>? GetVoicePacket
    {
        add => _signalrService.GetVoicePacket += value;
        remove => _signalrService.GetVoicePacket -= value;
    }
}
