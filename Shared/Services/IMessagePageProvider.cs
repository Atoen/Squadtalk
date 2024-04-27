using Shared.Data;

namespace Shared.Services;

public interface IMessagePageProvider
{
    Task<List<IChatMessage>> GetPageAsync(ChannelId channelId, MessageCursor cursor, CancellationToken cancellationToken);
}