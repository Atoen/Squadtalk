using Shared.Data.TypedIds;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal class TextChatService : ITextChatService
{
    event Action<ChannelId, MessageModel>? ITextChatService.MessageReceived { add { } remove { } }

    public Task<IList<MessageModel>> GetMessagePageAsync(ChannelId channelId, CancellationToken cancellationToken) => Task.FromResult<IList<MessageModel>>(Array.Empty<MessageModel>());

    public Task SendMessageAsync(string message, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void StartedTyping(ChannelId channelId) { }

    public void StoppedTyping() { }
}
