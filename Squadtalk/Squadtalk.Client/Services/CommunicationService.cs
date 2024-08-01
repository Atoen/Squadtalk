using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs;
using Shared.Services;

namespace Squadtalk.Client.Services;

public class CommunicationService(ISignalrService signalrService) : ICommunicationService
{
    public string ConnectionStatus => signalrService.ConnectionStatus;
    public bool Connected => signalrService.Connected;

    public Task ConnectAsync()
    {
        return signalrService.ConnectAsync();
    }

    public Task SendMessageAsync(string content, ChannelId channelId, CancellationToken cancellationToken)
    {
        return signalrService.SendMessageAsync(content, channelId, cancellationToken);
    }

    public Task<RoomTokenDto?> StartVoiceCallAsync(ChannelId channelId)
    {
        return signalrService.StartVoiceCallAsync(channelId);
    }

    public Task<RoomTokenDto?> AcceptCallAsync(ChannelId channelId)
    {
        return signalrService.AcceptCallAsync(channelId);
    }

    public Task DeclineCallAsync(ChannelId channelId)
    {
        return signalrService.DeclineCallAsync(channelId);
    }

    public Task<bool> ChannelHasActiveCall(ChannelId channelId)
    {
        return signalrService.ChannelHasActiveCall(channelId);
    }

    public Task<bool> ChangeChannelNameAsync(string? newName, ChannelId channelId)
    {
        return signalrService.ChangeChannelNameAsync(newName, channelId);
    }

    public Task<TimeSpan> MeasureClientDelayAsync()
    {
        return signalrService.MeasureClientDelayAsync();
    }

    public event Func<IChatMessage, Task>? MessageReceived
    {
        add => signalrService.MessageReceived += value;
        remove => signalrService.MessageReceived -= value;
    }

    public event Func<IChatUser, Task>? UserConnected
    {
        add => signalrService.UserConnected += value;
        remove => signalrService.UserConnected -= value;
    }

    public event Func<IChatUser, Task>? UserDisconnected
    {
        add => signalrService.UserDisconnected += value;
        remove => signalrService.UserDisconnected -= value;
    }

    public event Func<IEnumerable<IChatUser>, bool, Task>? ConnectedUsersReceived
    {
        add => signalrService.ConnectedUsersReceived += value;
        remove => signalrService.ConnectedUsersReceived -= value;
    }

    public event Func<string, Task>? ConnectionStatusChanged
    {
        add => signalrService.ConnectionStatusChanged += value;
        remove => signalrService.ConnectionStatusChanged -= value;
    }

    public event Func<IEnumerable<IChatChannel>, Task>? ChannelsReceived
    {
        add => signalrService.ChannelsReceived += value;
        remove => signalrService.ChannelsReceived -= value;
    }
    
    public event Func<IChatChannel, Task>? AddedToChannel
    {
        add => signalrService.AddedToChannel += value;
        remove => signalrService.AddedToChannel -= value;
    }

    public event Func<ChannelId, string?, Task>? ChannelNameChanged
    {
        add => signalrService.ChannelNameChanged += value;
        remove => signalrService.ChannelNameChanged-= value;
    }

    public event Func<ChannelId, UserId, Task>? IncomingCall
    {
        add => signalrService.IncomingCall += value;
        remove => signalrService.IncomingCall -= value;
    }

    public event Func<ChannelId, IChatUser, Task>? CallAccepted
    {
        add => signalrService.CallAccepted += value;
        remove => signalrService.CallAccepted -= value;
    }

    public event Func<IChatUser, ChannelId, Task>? CallDeclined
    {
        add => signalrService.CallDeclined += value;
        remove => signalrService.CallDeclined -= value;
    }

    public event Func<ChannelId, Task>? CallEnded
    {
        add => signalrService.CallEnded += value;
        remove => signalrService.CallEnded -= value;
    }

    public event Func<string, Task>? CallFailed
    {
        add => signalrService.CallFailed += value;
        remove => signalrService.CallFailed -= value;
    }
}
