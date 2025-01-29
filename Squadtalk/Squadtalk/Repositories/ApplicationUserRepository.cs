using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Shared.Data.TypedIds;
using Shared.Extensions;
using Squadtalk.Data;
using Squadtalk.Data.Entities;

namespace Squadtalk.Repositories;

internal class ApplicationUserRepository(
    ApplicationDbContext dbContext,
    ILogger<ApplicationUserRepository> logger) : RepositoryBase(dbContext, logger)
{
    public Task<ApplicationUser?> FindUserById(ClaimsPrincipal? principal, GroupInclusionOption groupInclusionOption = GroupInclusionOption.DontInclude)
    {
        if (principal?.GetClaimValue(ClaimTypes.NameIdentifier) is not { } claim)
        {
            return Task.FromResult<ApplicationUser?>(null);
        }

        return UserId.TryParse(claim, out var userId)
            ? FindUserById(userId, groupInclusionOption)
            : Task.FromResult<ApplicationUser?>(null);
    }

    public Task<ApplicationUser?> FindUserById(UserId userId, GroupInclusionOption groupInclusionOption = GroupInclusionOption.DontInclude)
    {
        return groupInclusionOption switch
        {
            GroupInclusionOption.DontInclude => UserByIdAsync(DbContext, userId),
            GroupInclusionOption.Include => UserByIdWithGroupsAsync(DbContext, userId),
            GroupInclusionOption.IncludeWithParticipants => UserByIdWithFullGroupsAsync(DbContext, userId),
            _ => throw new ArgumentOutOfRangeException(nameof(groupInclusionOption), groupInclusionOption, null)
        };
    }

    public async Task<ApplicationUser?> FindUserByNameAsync(string username)
    {
        var normalizedUsername = username.ToUpperInvariant();
        return await UserByNormalizedNameAsync(DbContext, normalizedUsername);
    }

    public async Task SetLastSeen(ApplicationUser user, DateTimeOffset lastSeen)
    {
        user.LastSeen = lastSeen;
        DbContext.Update(user);

        await SaveChangesAsync();
    }

    public async Task<List<ApplicationUser>> GetUserListAsync(List<UserId> userIds)
    {
        var users = UserListByIdAsync(DbContext, userIds);

        return await users.ToListAsync();
    }

    private static readonly Func<ApplicationDbContext, UserId, Task<ApplicationUser?>> UserByIdAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, UserId userId) => context.Users
                .SingleOrDefault(x => x.Id == userId));

    private static readonly Func<ApplicationDbContext, UserId, Task<ApplicationUser?>> UserByIdWithGroupsAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, UserId userId) => context.Users
                .AsSplitQuery()
                .Include(x => x.GroupParticipants)
                .ThenInclude(x => x.Group)
                .ThenInclude(x => x.Participants)
                .SingleOrDefault(x => x.Id == userId));

    private static readonly Func<ApplicationDbContext, UserId, Task<ApplicationUser?>> UserByIdWithFullGroupsAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, UserId userId) => context.Users
                .AsSplitQuery()
                .Include(x => x.GroupParticipants)
                .ThenInclude(x => x.Group)
                .ThenInclude(x => x.Participants)
                .ThenInclude(x => x.User)
                .SingleOrDefault(x => x.Id == userId));

    private static readonly Func<ApplicationDbContext, List<UserId>, IAsyncEnumerable<ApplicationUser>> UserListByIdAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, List<UserId> userIds) => context.Users
                .Where(x => userIds.Contains(x.Id)));

    private static readonly Func<ApplicationDbContext, string, Task<ApplicationUser?>> UserByNormalizedNameAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, string normalizedUsername) => context.Users
                .FirstOrDefault(x => x.NormalizedUserName == normalizedUsername));
}

public enum GroupInclusionOption
{
    DontInclude,
    Include,
    IncludeWithParticipants
}
