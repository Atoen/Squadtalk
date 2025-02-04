using Shared.Data.TypedIds;
using Shared.Models;

namespace Shared.Services;

public interface ITextChatService
{
    event Action<ChatModel, MessageModel>? MessageReceived;

    Task<IList<MessageModel>> GetMessagePageAsync(GroupId groupId, CancellationToken cancellationToken);

    Task SendMessageAsync(string message, CancellationToken cancellationToken = default);

    void StartedTyping(GroupId groupId);

    void StoppedTyping();
}
