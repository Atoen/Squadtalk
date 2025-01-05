using MessagePack;
using Shared.Data.TypedIds;

namespace Shared.DTOs.Chat;

[MessagePackObject]
public class RemoveFriendDto
{
    [Key(0)]
    public required UserId FriendId { get; init; }
}
