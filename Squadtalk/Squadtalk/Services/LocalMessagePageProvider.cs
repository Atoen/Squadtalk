using Microsoft.EntityFrameworkCore;
using Shared.Data;
using Shared.Services;
using Squadtalk.Data;

namespace Squadtalk.Services;

public class LocalMessagePageProvider(ApplicationDbContext dbContext) : IMessagePageProvider
{
    public async Task<List<IChatMessage>> GetPageAsync(ChannelId channelId, MessageCursor cursor, CancellationToken cancellationToken)
    {
        var dateCursor = cursor == default
            ? default
            : new DateTimeOffset(cursor.Value, TimeSpan.Zero);
        
        var messages = cursor == default
            ? await dbContext.Messages
                .AsNoTracking()
                .OrderByDescending(m => m.Timestamp)
                .Where(m => m.ChannelId == channelId)
                .Take(20)
                .Include(m => m.Author)
                .Reverse()
                .ToListAsync(cancellationToken)
            
            : await dbContext.Messages
                .AsNoTracking()
                .OrderByDescending(m => m.Timestamp)
                .Where(m => m.ChannelId == channelId)
                .Where(m => m.Timestamp < dateCursor)
                .Take(20)
                .Include(m => m.Author)
                .Reverse()
                .ToListAsync(cancellationToken);
        
        return messages.Cast<IChatMessage>().ToList();
    }
}