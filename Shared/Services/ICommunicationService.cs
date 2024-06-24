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
    
    event Func<IChatUser, CallOfferId, Task>? IncomingCall;
    event Func<CallOfferId, Task>? CallAccepted;
    event Func<CallOfferId, Task>? CallDeclined;
    event Func<CallId, Task>? CallEnded;
    event Func<string, Task>? CallFailed;
    event Func<IEnumerable<IChatUser>, CallId, Task>? GetCallUsers;
    event Func<VoicePacketDto, Task>? GetVoicePacket;
    
    const string Online = "Online";
    const string Connecting = "Connecting";
    const string Reconnecting = "Reconnecting";
    const string Disconnected = "Disconnected";
    const string Offline = "Offline";
    
    string ConnectionStatus { get; }
    
    bool Connected { get; }
    
    Task ConnectAsync();
    
    Task SendMessageAsync(string content, ChannelId channelId, CancellationToken cancellationToken);

    Task<CallOfferId?> StartVoiceCallAsync(UserId id);

    Task EndCallAsync(CallId id);

    Task AcceptCallAsync(CallOfferId id);
    
    Task DeclineCallAsync(CallOfferId id);

    Task StreamDataAsync(CallId callId, IAsyncEnumerable<byte[]> stream, CancellationToken cancellationToken);
}