using System.ComponentModel.DataAnnotations;

namespace Squadtalk.Client.Models;

public class UserCredentials
{
    [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username is too short")]
    [MaxLength(24, ErrorMessage = "Username is too long")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(3, ErrorMessage = "Password is too short")]
    public string? Password { get; set; }
}