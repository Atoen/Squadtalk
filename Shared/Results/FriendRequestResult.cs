using Shared.Data.TypedIds;

namespace Shared.Results;

public abstract record SendFriendRequestResult(FriendRequestResult Value)
{
    public sealed record Success(FriendRequestId RequestId) : SendFriendRequestResult(FriendRequestResult.Success);

    public sealed record RecipientNotFound() : SendFriendRequestResult(FriendRequestResult.RecipientNotFound);

    public sealed record SelfRequest() : SendFriendRequestResult(FriendRequestResult.SelfRequest);

    public sealed record RequestAlreadyPending() : SendFriendRequestResult(FriendRequestResult.RequestAlreadyPending);

    public sealed record AlreadyFriends() : SendFriendRequestResult(FriendRequestResult.AlreadyFriends);

    public sealed record Error() : SendFriendRequestResult(FriendRequestResult.Error);
}
