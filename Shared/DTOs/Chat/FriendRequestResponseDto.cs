using System.ComponentModel.DataAnnotations;
using Shared.Data.TypedIds;

namespace Shared.DTOs.Chat;

public class FriendRequestResponseDto
{
    [Required]
    public required UserId RespondingUserId { get; init; }

    [Required]
    public required FriendRequestId FriendRequestId { get; init; }

    [Required]
    public required bool Accepted { get; init; }
}
