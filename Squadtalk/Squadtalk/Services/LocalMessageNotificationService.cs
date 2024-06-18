using Shared.Extensions;
using Squadtalk.Data;

namespace Squadtalk.Services;

public class LocalMessageNotificationService
{
    public event Func<Message, Task>? MessageSent;

    public Task NotifyAboutMessageAsync(Message message)
    {
        return MessageSent.TryInvoke(message);
    }
}