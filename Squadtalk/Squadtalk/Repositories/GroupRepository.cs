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
        return ChannelByIdAsync(DbContext, groupId);
    }

    public async Task<bool> UserParticipatesInGroupAsync(UserId userId, GroupId groupId)
    {
        if (groupId == GroupChatModel.GlobalChatId)
        {
            return true;
        }

        var channel = await GetGroupAsync(groupId);

        return channel?.UserParticipatesInChannel(userId) ?? false;
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

    public async Task<bool> AddUsersToGroupAsync(GroupId groupId, ChatUser addingUser, List<ChatUser> users)
    {
        if (users.Count == 0)
        {
            return false;
        }

        var channel = await ChannelByIdAsync(DbContext, groupId);
        if (channel is null)
        {
            return false;
        }

        var existingUserIds = channel.Participants.Select(x => x.UserId).ToHashSet();
        var newUsers = users
            .Where(user => !existingUserIds.Contains(user.Id))
            .ToList();

        if (newUsers.Count == 0)
        {
            return false;
        }

        var addedParticipants = newUsers.Select(x => x.ToGroupParticipant(channel, addingUser));

        foreach (var participant in addedParticipants)
        {
            channel.Participants.Add(participant);
        }

        return await UpdateGroupAsync(channel);
    }

    public async Task<bool> AddGroupAsync(Group group, CancellationToken cancellationToken = default)
    {
        DbContext.Channels.Add(group);

        return await SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UpdateGroupAsync(Group group, CancellationToken cancellationToken = default)
    {
        DbContext.Channels.Update(group);

        return await SaveChangesAsync(cancellationToken);
    }

    private static readonly Func<ApplicationDbContext, GroupId, Task<Group?>> ChannelByIdAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, GroupId channelId) => context.Channels
                .Include(x => x.GroupCreator)
                .Include(x => x.Participants)
                .SingleOrDefault(x => x.Id == channelId));
}
