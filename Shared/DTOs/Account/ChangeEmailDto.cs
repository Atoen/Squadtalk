using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Account;

public class ChangeEmailDto
{
    [Required]
    public required string UserId { get; init; }

    [Required]
    [EmailAddress]
    public required string NewEmail { get; init; }

    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; init; }
}
