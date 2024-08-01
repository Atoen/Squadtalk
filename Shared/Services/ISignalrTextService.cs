using Shared.Data.TypedIds;
using Shared.DTOs;

namespace Shared.Services;

public interface ISignalrTextService
{
    event Func<MessageDto, Task>? MessageReceived;
    event Func<UserDto, Task>? UserConnected;
    event Func<UserDto, Task>? UserDisconnected;
    event Func<IEnumerable<UserDto>, bool, Task>? ConnectedUsersReceived;
    event Func<string, Task>? ConnectionStatusChanged;
    event Func<IEnumerable<ChannelDto>, Task>? ChannelsReceived;
    event Func<ChannelDto, Task>? AddedToChannel;
    event Func<ChannelId, string?, Task>? ChannelNameChanged;
    
    Task SendMessageAsync(string message, ChannelId channelId, CancellationToken cancellationToken = default);

    Task<bool> ChangeChannelNameAsync(string? newName, ChannelId channelId);
}
