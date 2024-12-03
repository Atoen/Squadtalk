using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Account;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;
}
