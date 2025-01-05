using Microsoft.AspNetCore.SignalR.Client;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Extensions;

namespace Squadtalk.Client.Services.SignalR;

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
