using Shared.Data.TypedIds;
using Shared.Models;

namespace Shared.Services;

public interface ITextChatService
{
    event Action<ChannelId, MessageModel>? MessageReceived;

    Task<IList<MessageModel>> GetMessagePageAsync(ChannelId channelId, CancellationToken cancellationToken);

    Task SendMessageAsync(string message, CancellationToken cancellationToken = default);

    void StartedTyping(ChannelId channelId);

    void StoppedTyping();
}
