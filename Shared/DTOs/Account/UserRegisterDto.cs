using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Account;

public class UserRegisterDto
{
    [Required]
    public string Username { get; set; } = default!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;
}
