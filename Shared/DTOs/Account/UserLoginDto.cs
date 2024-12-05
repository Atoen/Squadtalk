using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Account;

public class UserLoginDto
{
    [Required]
    public required string Username { get; init; }

    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; init; }

    public bool Remember { get; init; }
}
