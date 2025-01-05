using MessagePack;
using Shared.Results;

namespace Shared.DTOs.Chat;

[MessagePackObject]
public class FriendRequestResultDto
{
    [Key(0)]
    public required FriendRequestResult Status { get; init; }

    [Key(1)]
    public required PendingFriendRequestDto? FriendRequest { get; init; }
}
