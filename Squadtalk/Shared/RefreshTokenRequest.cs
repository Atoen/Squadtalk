namespace Squadtalk.Shared;

public class RefreshTokenRequest
{
    public required string Username { get; set; }
    public required string Token { get; set; }
}