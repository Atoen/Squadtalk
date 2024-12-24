using System.ComponentModel.DataAnnotations;
using Shared.Data.TypedIds;

namespace Shared.DTOs.Chat;

public class RemoveFriendDto
{
    [Required]
    public required UserId FriendId { get; init; }
}
