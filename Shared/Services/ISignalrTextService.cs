using Shared.Data.TypedIds;
using Shared.DTOs;

namespace Shared.Services;

public interface ISignalrTextService
{
    event Func<MessageDto, Task>? MessageReceived;
    event Func<UserDto, Task>? UserConnected;
    event Func<UserDto, Task>? UserDisconnected;
    event Func<IEnumerable<UserDto>, Task>? ConnectedUsersReceived;
    event Func<string, Task>? ConnectionStatusChanged;
    event Func<IEnumerable<ChannelDto>, Task>? ChannelsReceived;
    event Func<ChannelDto, Task>? AddedToChannel; 
    
    Task SendMessageAsync(string message, ChannelId channelId, CancellationToken cancellationToken = default);
}
