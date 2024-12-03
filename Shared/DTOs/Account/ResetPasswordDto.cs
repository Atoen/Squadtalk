using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Account;

public class ResetPasswordDto
{
    [Required]
    public string UserId { get; set; } = default!;

    [Required]
    public string NewPassword { get; set; } = default!;

    [Required]
    public string Code { get; set; } = default!;
}
