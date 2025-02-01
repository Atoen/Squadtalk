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
    GroupRepository groupRepository,
    ILogger<MessageRepository> logger)
    : RepositoryBase(dbContext, logger)
{
    private const int PageSize = 20;

    public async Task<List<Message>> GetPageAsync(GroupId groupId, TextChannelCursor cursor, CancellationToken cancellationToken = default)
    {
        var timestamp = new DateTimeOffset(cursor.Value, TimeSpan.Zero);

        var page = cursor == default
            ? MessageFirstPageAsync(DbContext, groupId)
            : MessagePageByCursorAsync(DbContext, groupId, timestamp);

        return await page.ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<GroupId, int>> GetUnreadMessageCountPerGroupAsync(UserId userId)
    {
        // var groupIds = groups.Select(x => x.Id).ToList();
        // var grouping = UnreadMessagesPerGroupAsync(DbContext, groupIds, since);
        //
        // return await grouping.ToDictionaryAsync(g => g.Key, g => g.Count());

        // Console.WriteLine(new string('\n', 20));
        //
        // var a = DbContext.Messages
        //     .Join(
        //         DbContext.GroupParticipants,
        //         message => message.GroupId,
        //         participant => participant.GroupId,
        //         (message, participant) => new { message, participant })
        //     .Where(x => x.participant.UserId == userId && (x.participant.LastSeen == null || x.participant.LastSeen < x.message.Timestamp))
        //     .GroupBy(x => x.message.GroupId);
        //
        // Console.WriteLine(a.ToQueryString());
        //
        // var b = await a.ToDictionaryAsync(x => x.Key, x => x.Count());
        //
        // Console.WriteLine(new string('\n', 20));
        //
        // return b;

        return [];
    }

    public async Task<Message?> AddMessageAsync(
        ChatUser author,
        string content,
        GroupId groupId,
        Embed? embed = null,
        CancellationToken cancellationToken = default)
    {
        var message = new Message
        {
            Author = author,
            Content = content,
            GroupId = groupId,
            Timestamp = DateTimeOffset.Now,
            Embed = embed
        };

        var added = await AddMessageAsync(message, cancellationToken);

        return added ? message : null;
    }

    public async Task<bool> AddMessageAsync(Message message, CancellationToken cancellationToken = default)
    {
        DbContext.Messages.Add(message);
        if (message.GroupId != ChatModel.GlobalChatId)
        {
            var group = await groupRepository.GetGroupAsync(message.GroupId);
            group?.WithLastMessage(message);
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

    private static readonly Func<ApplicationDbContext, List<GroupId>, DateTimeOffset, IAsyncEnumerable<IGrouping<GroupId, Message>>>
        UnreadMessagesPerGroupAsync = EF.CompileAsyncQuery(
            (ApplicationDbContext context, List<GroupId> groupIds, DateTimeOffset since) => context.Messages
                .AsNoTracking()
                .Where(x => groupIds.Contains(x.GroupId))
                .Where(x => x.Timestamp > since)
                .GroupBy(x => x.GroupId));

    private static readonly Func<ApplicationDbContext, GroupId, IAsyncEnumerable<Message>> MessageFirstPageAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, GroupId groupId) => context.Messages
                .AsNoTracking()
                .Where(x => x.GroupId == groupId)
                .OrderByDescending(x => x.Timestamp)
                .Take(PageSize)
                .Include(x => x.Author)
                .Reverse());

    private static readonly Func<ApplicationDbContext, GroupId, DateTimeOffset, IAsyncEnumerable<Message>> MessagePageByCursorAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, GroupId groupId, DateTimeOffset cursor) => context.Messages
                .AsNoTracking()
                .Where(x => x.GroupId == groupId)
                .OrderByDescending(x => x.Timestamp)
                .Where(x => x.Timestamp < cursor)
                .Take(PageSize)
                .Include(x => x.Author)
                .Reverse());
}
