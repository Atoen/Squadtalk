using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Account;

public class UsernameChangeResultDto
{
    [Required]
    public required string ChangedUsername { get; init; }
}
