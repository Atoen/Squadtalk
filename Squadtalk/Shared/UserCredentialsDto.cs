namespace Squadtalk.Shared;

public class UserCredentialsDto
{
	public required string Username { get; set; }
	public required string PasswordHash { get; set; }
}
