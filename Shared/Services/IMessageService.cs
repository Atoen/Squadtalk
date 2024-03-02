using Shared.Models;

namespace Shared.Services;

public interface IMessageService
{
    event Func<string, Task>? MessageReceived;

    Task<IList<MessageModel>> GetMessagePageAsync(string channelId, CancellationToken cancellationToken);

    Task SendMessageAsync(string message, CancellationToken cancellationToken = default);
}