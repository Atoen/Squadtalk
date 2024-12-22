using Shared.Data.TypedIds;
using Shared.Models;

namespace Shared.Services;

public interface ITextChatService
{
    event Func<ChannelId, MessageModel, Task>? MessageReceived;

    Task<IList<MessageModel>> GetMessagePageAsync(ChannelId id, CancellationToken cancellationToken);

    Task SendMessageAsync(string message, CancellationToken cancellationToken = default);
}
