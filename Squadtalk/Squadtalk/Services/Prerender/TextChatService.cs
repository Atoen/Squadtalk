using Shared.Data.TypedIds;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal class TextChatService : ITextChatService
{
    event Func<ChannelId, MessageModel, Task>? ITextChatService.MessageReceived { add { } remove { } }

    public Task<IList<MessageModel>> GetMessagePageAsync(ChannelId id, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task SendMessageAsync(string message, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
