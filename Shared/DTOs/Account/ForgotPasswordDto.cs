using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Account;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }
}
