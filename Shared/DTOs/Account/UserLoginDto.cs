using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Account;

public class UserLoginDto
{
    [Required]
    public string Username { get; set; } = default!;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = default!;

    public bool Remember { get; set; }
}
