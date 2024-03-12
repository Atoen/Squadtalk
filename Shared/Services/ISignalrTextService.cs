using Shared.Data;
using Shared.DTOs;

namespace Shared.Services;

public interface ISignalrTextService
{
    event Func<MessageDto, Task>? MessageReceived;
    event Func<UserDto, Task>? UserConnected;
    event Func<UserDto, Task>? UserDisconnected;
    event Func<IEnumerable<UserDto>, Task>? ConnectedUsersReceived;
    event Func<string, Task>? ConnectionStatusChanged;
    event Func<IEnumerable<ChannelDto>, Task>? TextChannelsReceived;
    event Func<ChannelDto, Task>? AddedToTextChannel; 
    
    Task SendMessageAsync(string message, ChannelId id, CancellationToken cancellationToken = default);
    
    
}