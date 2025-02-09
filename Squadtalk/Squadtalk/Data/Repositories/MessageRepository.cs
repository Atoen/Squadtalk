using Microsoft.EntityFrameworkCore;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Extensions;
using Shared.Models;
using Squadtalk.Data.Entities;
using Squadtalk.Extensions;

namespace Squadtalk.Data.Repositories;

public class MessageRepository(
    ApplicationDbContext dbContext,
    GroupRepository groupRepository,
    ILogger<MessageRepository> logger)
    : RepositoryBase(dbContext, logger)
{
    private const int PageSize = 20;

    public async Task<List<Message>> GetPageAsync(GroupId groupId, TextChannelCursor cursor, CancellationToken cancellationToken = default)
    {
        var page = cursor == default
            ? MessageFirstPageAsync(DbContext, groupId)
            : MessagePageByCursorAsync(DbContext, groupId, cursor.Value);

        return await page.ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<GroupId, int>> GetUnreadMessageCountPerGroupAsync(UserId userId)
    {
        return await UnreadMessagesPerGroupAsync(DbContext, userId)
            .ToDictionaryAsync(x => x.GroupId, x => x.Unread);
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

    private readonly record struct UnreadMessagesInChannel(GroupId GroupId, int Unread);

    private static readonly Func<ApplicationDbContext, UserId, IAsyncEnumerable<UnreadMessagesInChannel>> UnreadMessagesPerGroupAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, UserId userId) => context.Messages
                .AsNoTracking()
                .Select(x => new { x.GroupId, x.Id })
                .Join(
                    context.GroupParticipants.Select(x => new { x.GroupId, x.UserId, x.LastMessageSeenId }),
                    message => message.GroupId,
                    participant => participant.GroupId,
                    (message, participant) => new { message, participant })
                .Where(x => x.participant.UserId == userId)
                .Where(x => x.participant.LastMessageSeenId < x.message.Id)
                .GroupBy(x => x.message.GroupId)
                .Select(x => new UnreadMessagesInChannel(x.Key, x.Count())));


    private static readonly Func<ApplicationDbContext, GroupId, IAsyncEnumerable<Message>> MessageFirstPageAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, GroupId groupId) => context.Messages
                .AsNoTracking()
                .Where(x => x.GroupId == groupId)
                .OrderByDescending(x => x.Timestamp)
                .Take(PageSize)
                .Include(x => x.Author)
                .Reverse());

    private static readonly Func<ApplicationDbContext, GroupId, MessageId, IAsyncEnumerable<Message>> MessagePageByCursorAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, GroupId groupId, MessageId cursor) => context.Messages
                .AsNoTracking()
                .Where(x => x.GroupId == groupId)
                .Where(x => x.Id < cursor)
                .OrderByDescending(x => x.Timestamp)
                .Take(PageSize)
                .Include(x => x.Author)
                .Reverse());
}
