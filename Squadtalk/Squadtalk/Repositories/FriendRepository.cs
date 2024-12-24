using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Shared.Data.TypedIds;
using Shared.Results;
using Squadtalk.Data;
using Squadtalk.Data.Entities;

namespace Squadtalk.Repositories;

public class FriendRepository(ApplicationDbContext dbContext, ILogger<FriendRepository> logger) : RepositoryBase(dbContext, logger)
{
    private static readonly object ErrorValue = -1;

    public async Task<List<ApplicationUser>> GetUserFriendsAsync(UserId userId)
    {
        return await UserFriendsAsync(DbContext, userId).ToListAsync();
    }

    public async Task<List<FriendRequest>> GetUserPendingFriendRequests(UserId userId)
    {
        return await UserPendingFriendRequests(DbContext, userId).ToListAsync();
    }

    public async Task<FriendRequestResult> AddFriendRequest(
        UserId senderId, string recipientUsername, CancellationToken cancellationToken)
    {
        await using var connection = DbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT send_friend_request(@requesterId, @recipientUsername)";

        command.AddParameter("@requesterId", senderId.Value)
               .AddParameter("@recipientUsername", recipientUsername);

        var result = await command.ExecuteScalarAsync(cancellationToken);

        return (FriendRequestResult) (result ?? ErrorValue);
    }

    public async Task<FriendRequestResponseResult> RespondToFriendRequestAsync(
        UserId respondingId, FriendRequestId friendRequestId, bool isAccepted, CancellationToken cancellationToken)
    {
        await using var connection = DbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT respond_to_friend_request(@respondingId, @friendRequestId, @isAccepted)";

        command.AddParameter("@respondingId", respondingId.Value)
               .AddParameter("@friendRequestId", friendRequestId.Value)
               .AddParameter("@isAccepted", isAccepted);

        var result = await command.ExecuteScalarAsync(cancellationToken);

        return (FriendRequestResponseResult) (result ?? ErrorValue);
    }

    public async Task<RemoveFriendResult> RemoveFriendAsync(
        UserId removingUser, UserId friendToRemove, CancellationToken cancellationToken)
    {
        await using var connection = DbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT remove_friend(@removingUserId, @friendId)";

        command.AddParameter("@removingUserId", removingUser.Value)
               .AddParameter("@friendId", friendToRemove.Value);

        var result = await command.ExecuteScalarAsync(cancellationToken);

        return (RemoveFriendResult) (result ?? ErrorValue);
    }

    private static readonly Func<ApplicationDbContext, UserId, IAsyncEnumerable<ApplicationUser>> UserFriendsAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, UserId userId) => context.Friendships
                .AsNoTracking()
                .Where(x => x.User1.Id == userId || x.User2.Id == userId)
                .Select(x => x.User1.Id == userId ? x.User2 : x.User1));

    private static readonly Func<ApplicationDbContext, UserId, IAsyncEnumerable<FriendRequest>> UserPendingFriendRequests =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, UserId userId) => context.FriendRequests
                .Where(x => x.IsAccepted == null)
                .Where(x => x.Recipient.Id == userId || x.Requester.Id == userId)
                .Include(x => x.Requester)
                .Include(x => x.Recipient));
}

file static class DbCommandExtensions
{
    public static DbCommand AddParameter(this DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;

        command.Parameters.Add(parameter);

        return command;
    }
}
