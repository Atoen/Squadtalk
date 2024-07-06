using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs;

namespace Shared.Services;

public interface ICommunicationService
{
    event Func<IChatMessage, Task>? MessageReceived;
    
    event Func<IChatUser, Task>? UserConnected;
    event Func<IChatUser, Task>? UserDisconnected;
    event Func<IEnumerable<IChatUser>, Task>? ConnectedUsersReceived;
    event Func<string, Task>? ConnectionStatusChanged;
    event Func<IEnumerable<IChatChannel>, Task>? TextChannelsReceived;
    event Func<IChatChannel, Task>? AddedToTextChannel;
    
    event Func<ChannelId, UserId, Task>? IncomingCall;
    event Func<ChannelId, IChatUser, Task>? CallAccepted;
    event Func<IChatUser, ChannelId, Task>? CallDeclined;
    event Func<ChannelId, Task>? CallEnded;
    event Func<string, Task>? CallFailed;
    
    const string Online = "Online";
    const string Connecting = "Connecting";
    const string Reconnecting = "Reconnecting";
    const string Disconnected = "Disconnected";
    const string Offline = "Offline";
    
    string ConnectionStatus { get; }
    
    bool Connected { get; }
    
    Task ConnectAsync();
    
    Task SendMessageAsync(string content, ChannelId channelId, CancellationToken cancellationToken);

    Task<RoomTokenDto?> StartVoiceCallAsync(ChannelId channelId);

    Task<RoomTokenDto?> AcceptCallAsync(ChannelId channelId);
    
    Task DeclineCallAsync(ChannelId channelId);
}
