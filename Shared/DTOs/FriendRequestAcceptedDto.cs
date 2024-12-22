using MessagePack;
using Shared.DTOs.Chat;

namespace Shared.DTOs;

[MessagePackObject]
public class FriendRequestAcceptedDto
{
    [Key(0)]
    public UserDto AcceptingUser { get; set; } = default!;
}
