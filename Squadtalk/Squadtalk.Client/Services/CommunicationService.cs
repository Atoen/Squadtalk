using Shared.Data;
using Shared.Data.TypedIds;
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

    public Task<RoomTokenDto?> StartVoiceCallAsync(ChannelId channelId)
    {
        return _signalrService.StartVoiceCallAsync(channelId);
    }

    public Task<RoomTokenDto?> AcceptCallAsync(ChannelId channelId)
    {
        return _signalrService.AcceptCallAsync(channelId);
    }

    public Task DeclineCallAsync(ChannelId channelId)
    {
        return _signalrService.DeclineCallAsync(channelId);
    }

    public Task<bool> ChannelHasActiveCall(ChannelId channelId)
    {
        return _signalrService.ChannelHasActiveCall(channelId);
    }

    public Task<bool> ChangeChannelNameAsync(string? newName, ChannelId channelId)
    {
        return _signalrService.ChangeChannelNameAsync(newName, channelId);
    }

    public Task<TimeSpan> MeasureClientDelayAsync()
    {
        return _signalrService.MeasureClientDelayAsync();
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

    public event Func<IEnumerable<IChatChannel>, Task>? ChannelsReceived
    {
        add => _signalrService.ChannelsReceived += value;
        remove => _signalrService.ChannelsReceived -= value;
    }
    
    public event Func<IChatChannel, Task>? AddedToChannel
    {
        add => _signalrService.AddedToChannel += value;
        remove => _signalrService.AddedToChannel -= value;
    }

    public event Func<ChannelId, string?, Task>? ChannelNameChanged
    {
        add => _signalrService.ChannelNameChanged += value;
        remove => _signalrService.ChannelNameChanged-= value;
    }

    public event Func<ChannelId, UserId, Task>? IncomingCall
    {
        add => _signalrService.IncomingCall += value;
        remove => _signalrService.IncomingCall -= value;
    }

    public event Func<ChannelId, IChatUser, Task>? CallAccepted
    {
        add => _signalrService.CallAccepted += value;
        remove => _signalrService.CallAccepted -= value;
    }

    public event Func<IChatUser, ChannelId, Task>? CallDeclined
    {
        add => _signalrService.CallDeclined += value;
        remove => _signalrService.CallDeclined -= value;
    }

    public event Func<ChannelId, Task>? CallEnded
    {
        add => _signalrService.CallEnded += value;
        remove => _signalrService.CallEnded -= value;
    }

    public event Func<string, Task>? CallFailed
    {
        add => _signalrService.CallFailed += value;
        remove => _signalrService.CallFailed -= value;
    }
}
