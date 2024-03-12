using Shared.Data;
using Shared.Models;

namespace Shared.Services;

public interface IMessageService
{
    event Func<ChannelId, Task>? MessageReceived;

    Task<IList<MessageModel>> GetMessagePageAsync(ChannelId id, CancellationToken cancellationToken);

    Task SendMessageAsync(string message, CancellationToken cancellationToken = default);
}