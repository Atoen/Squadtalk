using Microsoft.AspNetCore.SignalR.Client;
using Shared.Data.TypedIds;
using Shared.DTOs;
using Shared.Extensions;
using Squadtalk.Client.SignalR;

namespace Squadtalk.Client.Services;

internal sealed partial class SignalrService : ISignalrTextService
{
    public event Func<MessageDto, Task>? MessageReceived;

    Task ISignalrTextService.SendMessageAsync(string message, ChannelId channelId, CancellationToken cancellationToken)
    {
        return _connection.SendAsync("SendMessage", message, channelId, cancellationToken);
    }

    private void RegisterTextHandlers()
    {
        _connection.On<MessageDto>("ReceiveMessage", message =>
            MessageReceived.TryInvoke(message));
    }
}
