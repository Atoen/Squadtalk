using System.ComponentModel.DataAnnotations;
using Shared.Data.TypedIds;

namespace Shared.DTOs.Chat;

public class FriendRequestDto
{
    [Required]
    public required string RecipientUsername { get; set; }

    [Required]
    public required UserId RequestingUserId { get; set; }
}
