using MessagePack;
using Shared.Data.TypedIds;

namespace Shared.DTOs.Chat;

[MessagePackObject]
public class FriendRequestResponseDto
{
    [Key(0)]
    public required FriendRequestId FriendRequestId { get; init; }

    [Key(1)]
    public required bool Accepted { get; init; }
}
