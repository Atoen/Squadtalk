using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Account;

public class ChangeUsernameDto
{
    [Required]
    public required string UserId { get; init; }

    [Required]
    public required string NewUsername { get; init; }

    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; init; }
}
