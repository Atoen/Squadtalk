using Microsoft.EntityFrameworkCore;
using Shared.Data.TypedIds;
using Shared.Results;
using Squadtalk.Data.Entities;

namespace Squadtalk.Data.Repositories;

public class FriendRepository(ApplicationDbContext dbContext, ILogger<FriendRepository> logger) : RepositoryBase(dbContext, logger)
{
    public async Task<List<ChatUser>> GetUserFriendsAsync(UserId userId)
    {
        Logger.LogInformation("Getting user friends");
        return await UserFriendsAsync(DbContext, userId).ToListAsync();
    }

    public async Task<List<UserId>> GetUserFriendIdsAsync(UserId userId)
    {
        Logger.LogInformation("Getting user friends ids");
        return await UserFriendIdsAsync(DbContext, userId).ToListAsync();
    }

    public async Task<List<FriendRequest>> GetUserPendingFriendRequests(UserId userId)
    {
        Logger.LogInformation("Getting user pending friend requests");
        return await UserPendingFriendRequests(DbContext, userId).ToListAsync();
    }

    public async Task<FriendRequest?> FindFriendRequestByIdAsync(FriendRequestId friendRequestId)
    {
        Logger.LogInformation("Getting friend request by id");
        return await FriendRequestByIdAsync(DbContext, friendRequestId);
    }

    public async Task<Friendship?> FindFriendshipById(int friendshipId)
    {
        Logger.LogInformation("Getting friendship by id");
        return await FriendshipByIdAsync(DbContext, friendshipId);
    }

    public async Task<SendFriendRequestResult> AddFriendRequestAsync(
        UserId senderId, string recipientUsername, CancellationToken cancellationToken)
    {
        var output = await DbContext.AddFriendRequest(senderId, recipientUsername).SingleAsync(cancellationToken);
        if (output is { Status: FriendRequestResult.Success, AddedRequestId: { } id })
        {
            return new SendFriendRequestResult.Success(new FriendRequestId(id));
        }

        return output.Status switch
        {
            FriendRequestResult.RecipientNotFound => new SendFriendRequestResult.RecipientNotFound(),
            FriendRequestResult.RequestAlreadyPending => new SendFriendRequestResult.RequestAlreadyPending(),
            FriendRequestResult.AlreadyFriends => new SendFriendRequestResult.AlreadyFriends(),
            FriendRequestResult.SelfRequest => new SendFriendRequestResult.SelfRequest(),
            _ => new SendFriendRequestResult.Error()
        };
    }

    public async Task<FriendRequest?> CancelFriendRequestAsync(UserId cancellingUserId, FriendRequestId friendRequestId, CancellationToken cancellationToken)
    {
        var friendRequest = await FriendRequestByIdAsync(DbContext, friendRequestId);
        if (friendRequest is null || friendRequest.Requester.Id != cancellingUserId)
        {
            return null;
        }

        await CancelFriendRequestById(DbContext, friendRequestId);

        return friendRequest;
    }

    public async Task<RespondToFriendRequestResult> RespondToFriendRequestAsync(
        UserId respondingId, FriendRequestId friendRequestId, bool isAccepted, CancellationToken cancellationToken)
    {
        var output = await DbContext.RespondToFriendRequest(respondingId, friendRequestId.Value, isAccepted).SingleAsync(cancellationToken);
        var requestingUserId = UserId.From(output.RequesterId ?? Guid.Empty);

        if (output is { Status: FriendRequestResponseResult.SuccessAccepted, AddedFriendshipId: { } friendshipId })
        {
            FriendRequestId? otherWayRequestId = output.OtherWayRequestId is { } value ? new FriendRequestId(value) : null;
            return new RespondToFriendRequestResult.Accepted(requestingUserId, friendshipId, otherWayRequestId);
        }

        return output.Status switch
        {
            FriendRequestResponseResult.SuccessRejected => new RespondToFriendRequestResult.Rejected(requestingUserId),
            FriendRequestResponseResult.InvalidResponse => new RespondToFriendRequestResult.InvalidResponse(),
            _ => new RespondToFriendRequestResult.Error()
        };
    }

    public async Task<RemoveFriendResult> RemoveFriendAsync(
        UserId removingUserId, UserId friendToRemoveId, CancellationToken cancellationToken)
    {
        var (id1, id2) = removingUserId.Value < friendToRemoveId.Value
            ? (removingUserId, friendToRemoveId)
            : (friendToRemoveId, removingUserId);

        var removedRows = await DbContext.Friendships
            .Where(x => x.User1Id == id1 && x.User2Id == id2)
            .ExecuteDeleteAsync(cancellationToken);

        return removedRows == 1 ? RemoveFriendResult.Success : RemoveFriendResult.BadRequest;
    }

    private static readonly Func<ApplicationDbContext, UserId, IAsyncEnumerable<ChatUser>> UserFriendsAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, UserId userId) => context.Friendships
                .AsNoTracking()
                .Where(x => x.User1Id == userId || x.User2Id == userId)
                .Select(x => x.User1Id == userId ? x.User2 : x.User1));

    private static readonly Func<ApplicationDbContext, UserId, IAsyncEnumerable<UserId>> UserFriendIdsAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, UserId userId) => context.Friendships
                .AsNoTracking()
                .Where(x => x.User1Id == userId || x.User2Id == userId)
                .Select(x => x.User1Id == userId ? x.User2Id : x.User1Id));

    private static readonly Func<ApplicationDbContext, UserId, IAsyncEnumerable<FriendRequest>> UserPendingFriendRequests =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, UserId userId) => context.FriendRequests
                .Where(x => x.IsAccepted == null)
                .Where(x => x.RecipientId == userId || x.RequesterId == userId)
                .Include(x => x.Requester)
                .Include(x => x.Recipient));

    private static readonly Func<ApplicationDbContext, FriendRequestId, Task<FriendRequest?>> FriendRequestByIdAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, FriendRequestId friendRequestId) => context.FriendRequests
                .Include(x => x.Recipient)
                .Include(x => x.Requester)
                .SingleOrDefault(x => x.Id == friendRequestId));

    private static readonly Func<ApplicationDbContext, int, Task<Friendship?>> FriendshipByIdAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, int friendshipId) => context.Friendships
                .AsNoTracking()
                .Include(x => x.User1)
                .Include(x => x.User2)
                .SingleOrDefault(x => x.Id == friendshipId));

    private static readonly Func<ApplicationDbContext, FriendRequestId, Task> CancelFriendRequestById =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, FriendRequestId friendRequestId) => context.FriendRequests
                .Where(x => x.Id == friendRequestId)
                .ExecuteDelete());
}
