using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

public class LocalMessagePageProvider : IMessagePageProvider
{
    private readonly Task<List<IChatMessage>> _empty = Task.FromResult(new List<IChatMessage>());

    public Task<List<IChatMessage>> GetPageAsync(ChannelId channelId, TextChannelCursor cursor, CancellationToken cancellationToken)
    {
        return _empty;
    }
}
