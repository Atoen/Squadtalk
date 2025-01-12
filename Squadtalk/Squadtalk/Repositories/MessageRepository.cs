using Microsoft.EntityFrameworkCore;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Extensions;
using Shared.Models;
using Squadtalk.Data;
using Squadtalk.Data.Entities;
using Squadtalk.Extensions;

namespace Squadtalk.Repositories;

public class MessageRepository(
    ApplicationDbContext dbContext,
    ChannelRepository channelRepository,
    ILogger<MessageRepository> logger)
    : RepositoryBase(dbContext, logger)
{
    private const int PageSize = 20;

    public async Task<List<Message>> GetPageAsync(ChannelId channelId, TextChannelCursor cursor, CancellationToken cancellationToken = default)
    {
        var timestamp = new DateTimeOffset(cursor.Value, TimeSpan.Zero);

        var page = cursor == default
            ? MessageFirstPageAsync(DbContext, channelId)
            : MessagePageByCursorAsync(DbContext, channelId, timestamp);

        return await page.ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<ChannelId, int>> GetUnreadMessageCountPerChannelAsync(List<Channel> channels, DateTimeOffset since)
    {
        var channelIds = channels.Select(x => x.Id).ToList();
        var grouping = UnreadMessagesPerChannelAsync(DbContext, channelIds, since);

        return await grouping.ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task<Message?> AddMessageAsync(
        ApplicationUser author,
        string content,
        ChannelId channelId,
        Embed? embed = null,
        CancellationToken cancellationToken = default)
    {
        var message = new Message
        {
            Author = author,
            Content = content,
            ChannelId = channelId,
            Timestamp = DateTimeOffset.Now,
            Embed = embed
        };

        var added = await AddMessageAsync(message, cancellationToken);

        return added ? message : null;
    }

    public async Task<bool> AddMessageAsync(Message message, CancellationToken cancellationToken = default)
    {
        DbContext.Messages.Add(message);
        if (message.ChannelId != GroupChatModel.GlobalChatId)
        {
            var channel = await channelRepository.GetChannelAsync(message.ChannelId);
            channel?.WithLastMessage(message);
        }

        return await SaveChangesAsync(cancellationToken);
    }

    private static DateTimeOffset CreateCursor(string? timestamp)
    {
        if (timestamp is null || timestamp.Length > 128)
        {
            return default;
        }

        if (!timestamp.TryFromBase64(out var converted, true))
        {
            return default;
        }

        return long.TryParse(converted, out var ticks)
            ? new DateTimeOffset(ticks, TimeSpan.Zero)
            : default;
    }

    private static readonly Func<ApplicationDbContext, List<ChannelId>, DateTimeOffset, IAsyncEnumerable<IGrouping<ChannelId, Message>>>
        UnreadMessagesPerChannelAsync = EF.CompileAsyncQuery(
            (ApplicationDbContext context, List<ChannelId> channelIds, DateTimeOffset since) => context.Messages
                .AsNoTracking()
                .Where(x => channelIds.Contains(x.ChannelId))
                .Where(x => x.Timestamp > since)
                .GroupBy(x => x.ChannelId));

    private static readonly Func<ApplicationDbContext, ChannelId, IAsyncEnumerable<Message>> MessageFirstPageAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, ChannelId channelId) => context.Messages
                .AsNoTracking()
                .Where(x => x.ChannelId == channelId)
                .OrderByDescending(x => x.Timestamp)
                .Take(PageSize)
                .Include(x => x.Author)
                .Reverse());

    private static readonly Func<ApplicationDbContext, ChannelId, DateTimeOffset, IAsyncEnumerable<Message>> MessagePageByCursorAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, ChannelId channelId, DateTimeOffset cursor) => context.Messages
                .AsNoTracking()
                .Where(x => x.ChannelId == channelId)
                .OrderByDescending(x => x.Timestamp)
                .Where(x => x.Timestamp < cursor)
                .Take(PageSize)
                .Include(x => x.Author)
                .Reverse());
}
