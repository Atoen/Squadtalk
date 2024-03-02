using Shared.DTOs;
using Shared.Extensions;
using Shared.Services;

namespace Squadtalk.Services;

public sealed class ServersideSignalrService : ISignalrService
{
#pragma warning disable CS0067

    public event Func<MessageDto, Task>? MessageReceived;
    public event Func<UserDto, Task>? UserConnected;
    public event Func<UserDto, Task>? UserDisconnected;
    public event Func<IEnumerable<UserDto>, Task>? ConnectedUsersReceived;
    public event Func<string, Task>? ConnectionStatusChanged;
    public event Func<IEnumerable<ChannelDto>, Task>? TextChannelsReceived;
    public event Func<ChannelDto, Task>? AddedToTextChannel;

#pragma warning restore CS0067
    
    public string ConnectionStatus { get; private set; } = ISignalrService.Offline;

    public bool Connected => false;

    public Task ConnectAsync()
    {
        ConnectionStatus = ISignalrService.Connecting;
        return ConnectionStatusChanged.TryInvoke(ConnectionStatus);
    }

    public Task SendMessageAsync(string message, string channelId, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException();
    }

    public Task StartVoiceCallAsync(List<string> invitedIds)
    {
        throw new InvalidOperationException();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}