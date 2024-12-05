using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Account;

public class ChangePasswordDto
{
    [Required]
    public required string UserId { get; init; }

    [Required]
    [DataType(DataType.Password)]
    public required string NewPassword { get; init; }

    [Required]
    [DataType(DataType.Password)]
    public required string CurrentPassword { get; init; }
}
