using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Shared.Data.TypedIds;
using Shared.Extensions;
using Squadtalk.Data;
using Squadtalk.Data.Entities;

namespace Squadtalk.Repositories;

public class UserRepository(ApplicationDbContext dbContext, ILogger<UserRepository> logger)  : RepositoryBase(dbContext, logger)
{
    public Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal? principal, ChannelsInclusionOption channelsInclusionOption = ChannelsInclusionOption.DontInclude)
    {
        if (principal?.GetClaimValue(ClaimTypes.NameIdentifier) is not { } claim)
        {
            return Task.FromResult<ApplicationUser?>(null);
        }

        return UserId.TryParse(claim, out var userId)
            ? GetUserAsync(userId, channelsInclusionOption)
            : Task.FromResult<ApplicationUser?>(null);
    }

    public Task<ApplicationUser?> GetUserAsync(UserId userId,  ChannelsInclusionOption channelsInclusionOption = ChannelsInclusionOption.DontInclude)
    {
        return channelsInclusionOption switch
        {
            ChannelsInclusionOption.DontInclude => UserByIdAsync(DbContext, userId),
            ChannelsInclusionOption.Include => UserByIdWithChannelsAsync(DbContext, userId),
            ChannelsInclusionOption.IncludeWithParticipants => UserByIdWithFullChannelsAsync(DbContext, userId),
            _ => throw new ArgumentOutOfRangeException(nameof(channelsInclusionOption), channelsInclusionOption, null)
        };
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

    private static readonly Func<ApplicationDbContext, UserId, Task<ApplicationUser?>> UserByIdWithChannelsAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, UserId userId) => context.Users
                .Include(x => x.Channels)
                .SingleOrDefault(x => x.Id == userId));

    private static readonly Func<ApplicationDbContext, UserId, Task<ApplicationUser?>> UserByIdWithFullChannelsAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, UserId userId) => context.Users
                .AsSplitQuery()
                .Include(x => x.Channels)
                .ThenInclude(x => x.Participants)
                .SingleOrDefault(x => x.Id == userId));

    private static readonly Func<ApplicationDbContext, List<UserId>, IAsyncEnumerable<ApplicationUser>> UserListByIdAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, List<UserId> userIds) => context.Users
                .Where(x => userIds.Contains(x.Id)));
}

public enum ChannelsInclusionOption
{
    DontInclude,
    Include,
    IncludeWithParticipants
}
