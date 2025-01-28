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
    public Task<ChatUser?> FindUserByIdAsync(ClaimsPrincipal? principal, ChannelsInclusionOption channelsInclusionOption = ChannelsInclusionOption.DontInclude)
    {
        if (principal?.GetClaimValue(ClaimTypes.NameIdentifier) is not { } claim)
        {
            return Task.FromResult<ChatUser?>(null);
        }

        return UserId.TryParse(claim, out var userId)
            ? FindUserByIdAsync(userId, channelsInclusionOption)
            : Task.FromResult<ChatUser?>(null);
    }

    public Task<ChatUser?> FindUserByIdAsync(UserId userId, ChannelsInclusionOption channelsInclusionOption = ChannelsInclusionOption.DontInclude)
    {
        return channelsInclusionOption switch
        {
            ChannelsInclusionOption.DontInclude => UserByIdAsync(DbContext, userId),
            ChannelsInclusionOption.Include => UserByIdWithChannelsAsync(DbContext, userId),
            ChannelsInclusionOption.IncludeWithParticipants => UserByIdWithFullChannelsAsync(DbContext, userId),
            _ => throw new ArgumentOutOfRangeException(nameof(channelsInclusionOption), channelsInclusionOption, null)
        };
    }

    public async Task<List<ChatUser>> GetUserListAsync(List<UserId> userIds)
    {
        var users = UserListByIdAsync(DbContext, userIds);

        return await users.ToListAsync();
    }

    private static readonly Func<ApplicationDbContext, UserId, Task<ChatUser?>> UserByIdAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, UserId userId) => context.ChatUsers
                .SingleOrDefault(x => x.Id == userId));

    private static readonly Func<ApplicationDbContext, UserId, Task<ChatUser?>> UserByIdWithChannelsAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, UserId userId) => context.ChatUsers
                .AsSplitQuery()
                .Include(x => x.GroupParticipants)
                .ThenInclude(x => x.Group)
                .ThenInclude(x => x.Participants)
                .SingleOrDefault(x => x.Id == userId));

    private static readonly Func<ApplicationDbContext, UserId, Task<ChatUser?>> UserByIdWithFullChannelsAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, UserId userId) => context.ChatUsers
                .AsSplitQuery()
                .Include(x => x.GroupParticipants)
                .ThenInclude(x => x.Group)
                .ThenInclude(x => x.Participants)
                .ThenInclude(x => x.User)
                .SingleOrDefault(x => x.Id == userId));

    private static readonly Func<ApplicationDbContext, List<UserId>, IAsyncEnumerable<ChatUser>> UserListByIdAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, List<UserId> userIds) => context.ChatUsers
                .Where(x => userIds.Contains(x.Id)));

    private static readonly Func<ApplicationDbContext, string, Task<ChatUser?>> UserByNameAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, string username) => context.ChatUsers
                .FirstOrDefault(x => x.Username == username));
}
