using Shared.Data;
using Shared.Data.TypedIds;

namespace Shared.Services;

public interface IMessagePageProvider
{
    Task<List<IChatMessage>> GetPageAsync(ChannelId channelId, MessageCursor cursor, CancellationToken cancellationToken);
}