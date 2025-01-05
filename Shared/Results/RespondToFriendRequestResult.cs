using Shared.Data.TypedIds;

namespace Shared.Results;

public abstract record RespondToFriendRequestResult(FriendRequestResponseResult Value)
{
    public sealed record Accepted(UserId RequesterId, int FriendshipId, FriendRequestId? OtherWayRequestId) : RespondToFriendRequestResult(FriendRequestResponseResult.SuccessAccepted);

    public sealed record Rejected(UserId RequesterId) : RespondToFriendRequestResult(FriendRequestResponseResult.SuccessRejected);

    public sealed record InvalidResponse() : RespondToFriendRequestResult(FriendRequestResponseResult.InvalidResponse);

    public sealed record Error() : RespondToFriendRequestResult(FriendRequestResponseResult.Error);
}
