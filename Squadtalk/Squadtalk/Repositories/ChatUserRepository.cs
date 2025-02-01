using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Shared.Data.TypedIds;
using Shared.Extensions;
using Squadtalk.Data;
using Squadtalk.Data.Entities;

namespace Squadtalk.Repositories;

public class ChatUserRepository(
    ApplicationDbContext dbContext,
    ILogger<ChatUserRepository> logger) : RepositoryBase(dbContext, logger)
{
    public Task<ChatUser?> FindUserByIdAsync(ClaimsPrincipal? principal, GroupInclusionOption groupInclusionOption = GroupInclusionOption.DontInclude)
    {
        if (principal?.GetClaimValue(ClaimTypes.NameIdentifier) is not { } claim)
        {
            return Task.FromResult<ChatUser?>(null);
        }

        return UserId.TryParse(claim, out var userId)
            ? FindUserByIdAsync(userId, groupInclusionOption)
            : Task.FromResult<ChatUser?>(null);
    }

    public Task<ChatUser?> FindUserByIdAsync(UserId userId, GroupInclusionOption groupInclusionOption = GroupInclusionOption.DontInclude)
    {
        return groupInclusionOption switch
        {
            GroupInclusionOption.DontInclude => UserByIdAsync(DbContext, userId),
            GroupInclusionOption.Include => UserByIdWithGroupsAsync(DbContext, userId),
            GroupInclusionOption.IncludeWithParticipants => UserByIdWithFullGroupsAsync(DbContext, userId),
            _ => throw new ArgumentOutOfRangeException(nameof(groupInclusionOption), groupInclusionOption, null)
        };
    }

    public async Task<List<ChatUser>> GetUserListAsync(List<UserId> userIds)
    {
        var users = UserListByIdAsync(DbContext, userIds);

        return await users.ToListAsync();
    }

    private static readonly Func<ApplicationDbContext, UserId, Task<ChatUser?>> UserByIdAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, UserId userId) => context.Users
                .Select(x => new ChatUser
                {
                    Username = x.UserName!,
                    Id = x.Id
                })
                .SingleOrDefault(x => x.Id == userId));

    private static readonly Func<ApplicationDbContext, UserId, Task<ChatUser?>> UserByIdWithGroupsAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, UserId userId) => context.Users
                .AsSplitQuery()
                .Include(x => x.GroupParticipants)
                .ThenInclude(x => x.Group)
                .ThenInclude(x => x.Participants)
                .Select(x => new ChatUser
                {
                    Username = x.UserName!,
                    Id = x.Id,
                    GroupParticipants = x.GroupParticipants
                })
                .SingleOrDefault(x => x.Id == userId));

    private static readonly Func<ApplicationDbContext, UserId, Task<ChatUser?>> UserByIdWithFullGroupsAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, UserId userId) => context.Users
                .AsSplitQuery()
                .Include(x => x.GroupParticipants)
                .ThenInclude(x => x.Group)
                .ThenInclude(x => x.Participants)
                .ThenInclude(x => x.User)
                .Select(x => new ChatUser
                {
                    Username = x.UserName!,
                    Id = x.Id,
                    GroupParticipants = x.GroupParticipants
                })
                .SingleOrDefault(x => x.Id == userId));

    private static readonly Func<ApplicationDbContext, List<UserId>, IAsyncEnumerable<ChatUser>> UserListByIdAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, List<UserId> userIds) => context.Users
                .Select(x => new ChatUser
                {
                    Username = x.UserName!,
                    Id = x.Id
                })
                .Where(x => userIds.Contains(x.Id)));

    private static readonly Func<ApplicationDbContext, string, Task<ChatUser?>> UserByNameAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, string username) => context.Users
                .Select(x => new ChatUser
                {
                    Username = x.UserName!,
                    Id = x.Id,
                })
                .FirstOrDefault(x => x.Username == username));
}

public enum GroupInclusionOption
{
    DontInclude,
    Include,
    IncludeWithParticipants
}
