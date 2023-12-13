using Shared.DTOs;

namespace Shared.Services;

public delegate Task ChannelsReceivedHandler(IEnumerable<ChannelDto> channel);

public delegate Task AddedToChannelHandler(ChannelDto channel);

public interface ISignalrService : IAsyncDisposable
{
    const string Online = "Online";
    const string Connecting = "Connecting";
    const string Reconnecting = "Reconnecting";
    const string Disconnected = "Disconnected";
    
    event Func<MessageDto, Task>? MessageReceived;
    
    event Func<UserDto, Task>? UserConnected;
    
    event Func<UserDto, Task>? UserDisconnected;
    
    event Func<IEnumerable<UserDto>, Task>? ConnectedUsersReceived;

    event Func<string, Task>? ConnectionStatusChanged;
    
    string ConnectionStatus { get; }
    
    Task ConnectAsync();
    
    Task SendMessageAsync(string message, string channelId, CancellationToken cancellationToken = default);
}