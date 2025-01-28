using Shared.Data.TypedIds;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal class TextChatService : ITextChatService
{
    event Action<GroupId, MessageModel>? ITextChatService.MessageReceived { add { } remove { } }

    public Task<IList<MessageModel>> GetMessagePageAsync(GroupId groupId, CancellationToken cancellationToken) => Task.FromResult<IList<MessageModel>>(Array.Empty<MessageModel>());

    public Task SendMessageAsync(string message, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void StartedTyping(GroupId groupId) { }

    public void StoppedTyping() { }
}
