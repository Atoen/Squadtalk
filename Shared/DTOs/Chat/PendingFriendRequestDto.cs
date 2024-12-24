using Shared.Data.TypedIds;

namespace Shared.DTOs.Chat;

public class PendingFriendRequestDto
{
    public required FriendRequestId Id { get; init; }

    public required UserDto Requester { get; init; }
    public required UserDto Recipient { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}
