using Shared.Data.TypedIds;
using Shared.DTOs;

namespace Squadtalk.Client.SignalR;

public interface ISignalrTextService
{
    event Func<MessageDto, Task>? MessageReceived;

    Task SendMessageAsync(string message, ChannelId channelId, CancellationToken cancellationToken = default);
}
