using MessagePack;
using Shared.Data.TypedIds;

namespace Shared.DTOs.Chat;

[MessagePackObject]
public class CancelFriendRequestDto
{
    [Key(0)]
    public required FriendRequestId RequestId { get; init; }
}
