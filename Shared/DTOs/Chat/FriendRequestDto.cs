using MessagePack;

namespace Shared.DTOs.Chat;

[MessagePackObject]
public class FriendRequestDto
{
    [Key(0)]
    public required string RecipientUsername { get; set; }
}
