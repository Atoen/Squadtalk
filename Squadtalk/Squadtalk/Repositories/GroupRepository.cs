using Microsoft.EntityFrameworkCore;
using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Models;
using Squadtalk.Data;
using Squadtalk.Data.Entities;
using Squadtalk.Extensions;

namespace Squadtalk.Repositories;

public class GroupRepository(
    ApplicationDbContext dbContext,
    ILogger<GroupRepository> logger)
    : RepositoryBase(dbContext, logger)
{
    public Task<Group?> GetGroupAsync(GroupId groupId)
    {
        return GroupByIdAsync(DbContext, groupId);
    }

    public async Task<bool> UserParticipatesInGroupAsync(UserId userId, GroupId groupId)
    {
        if (groupId == ChatModel.GlobalChatId)
        {
            return true;
        }

        var group = await GetGroupAsync(groupId);

        return group?.UserParticipatesInGroupAsync(userId) ?? false;
    }

    public async Task<Group?> CreateGroupAsync(ChatUser creatingUser, List<ChatUser> initialParticipants, CancellationToken cancellationToken = default)
    {
        if (initialParticipants.Count < 1 || initialParticipants.DistinctBy(x => x.Id).Count() != initialParticipants.Count)
        {
            return null;
        }

        var chatType = initialParticipants.Count == 2 ? ChatType.DirectMessage : ChatType.GroupChat;
        var group = new Group
        {
            Id = GroupId.New(),
            GroupCreator = creatingUser,
            ChatType = chatType
        };

        group.Participants = initialParticipants
            .Select(x => x.ToGroupParticipant(group, creatingUser))
            .ToList();

        var added = await AddGroupAsync(group, cancellationToken);

        return added ? group : null;
    }

    public async Task<bool> AddGroupAsync(Group group, CancellationToken cancellationToken = default)
    {
        DbContext.Channels.Add(group);

        return await SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteGroupAsync(Group group)
    {
        var deletedRows = await DeleteGroupByIdAsync(DbContext, group.Id);

        return deletedRows == 1;
    }

    public async Task<bool> UpdateGroupAsync(Group group, CancellationToken cancellationToken = default)
    {
        DbContext.Channels.Update(group);

        return await SaveChangesAsync(cancellationToken);
    }

    private static readonly Func<ApplicationDbContext, GroupId, Task<Group?>> GroupByIdAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, GroupId groupId) => context.Channels
                .Include(x => x.GroupCreator)
                .Include(x => x.Participants)
                .ThenInclude(x => x.User)
                .SingleOrDefault(x => x.Id == groupId));

    private static readonly Func<ApplicationDbContext, GroupId, Task<int>> DeleteGroupByIdAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, GroupId groupId) => context.Channels
                .Where(x => x.Id == groupId)
                .ExecuteDelete());
}
