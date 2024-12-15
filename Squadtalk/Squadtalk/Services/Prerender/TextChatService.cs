using Shared.Data.TypedIds;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal class TextChatService : ITextChatService
{
    public event Func<ChannelId, Task>? MessageReceived;

    public Task<IList<MessageModel>> GetMessagePageAsync(ChannelId id, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task SendMessageAsync(string message, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
