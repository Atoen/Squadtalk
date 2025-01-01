using Shared.Data.TypedIds;

namespace Shared.Models;

public class OutgoingFriendRequest : FriendRequest
{
    public UserModel To { get; set; } = default!;
}

public class IncomingFriendRequest : FriendRequest
{
    public UserModel From { get; set; } = default!;
}

public abstract class FriendRequest
{
    public DateTimeOffset CreatedAt { get; set; }
    public FriendRequestId Id { get; set; }
}


