using Shared.Data;
using Shared.Data.TypedIds;

namespace Shared.Services;

public interface IMessagePageProvider
{
    Task<List<IChatMessage>> GetPageAsync(ChannelId channelId, TextChannelCursor cursor, CancellationToken cancellationToken);
}