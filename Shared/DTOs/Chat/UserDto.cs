using MessagePack;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;

namespace Shared.DTOs.Chat;

[MessagePackObject]
public class UserDto : IChatUser
{
    [Key(0)] public string Username { get; set; } = default!;

    [Key(1)] public UserId Id { get; init; }

    [Key(2)] public UserStatus Status { get; set; }
}