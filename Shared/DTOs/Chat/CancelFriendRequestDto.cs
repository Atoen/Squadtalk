using System.ComponentModel.DataAnnotations;
using Shared.Data.TypedIds;

namespace Shared.DTOs.Chat;

public class CancelFriendRequestDto
{
    [Required]
    public required UserId CancellingUserId { get; init; }

    [Required]
    public required FriendRequestId RequestId { get; init; }
}
