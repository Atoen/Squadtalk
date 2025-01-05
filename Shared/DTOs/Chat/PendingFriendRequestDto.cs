using MessagePack;
using Shared.Data.TypedIds;

namespace Shared.DTOs.Chat;

[MessagePackObject]
public class PendingFriendRequestDto
{
    [Key(0)]
    public required FriendRequestId Id { get; init; }

    [Key(1)]
    public required UserDto Requester { get; init; }
    [Key(2)]
    public required UserDto Recipient { get; init; }

    [Key(3)]
    public required DateTimeOffset CreatedAt { get; init; }
}
