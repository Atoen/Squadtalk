namespace Shared.Data.Results;

public abstract record FriendRequestResult
{
    public sealed record Sent : FriendRequestResult;

    public sealed record NotFound : FriendRequestResult;

    public sealed record UserNotAccepting : FriendRequestResult;

    public sealed record NetworkError : FriendRequestResult;
}
