using Shared.DTOs;

namespace Shared.Services;

public interface ISignalrService : IAsyncDisposable
{
    const string Online = "Online";
    const string Connecting = "Connecting";
    const string Reconnecting = "Reconnecting";
    const string Disconnected = "Disconnected";
    const string Offline = "Offline";
    
    event Func<MessageDto, Task>? MessageReceived;
    
    event Func<UserDto, Task>? UserConnected;
    
    event Func<UserDto, Task>? UserDisconnected;
    
    event Func<IEnumerable<UserDto>, Task>? ConnectedUsersReceived;

    event Func<string, Task>? ConnectionStatusChanged;

    event Func<IEnumerable<ChannelDto>, Task>? TextChannelsReceived;

    event Func<ChannelDto, Task>? AddedToTextChannel; 
    
    string ConnectionStatus { get; }
    
    bool Connected { get; }
    
    Task ConnectAsync();
    
    Task SendMessageAsync(string message, string channelId, CancellationToken cancellationToken = default);

    Task StartVoiceCallAsync(List<string> invitedIds);
}