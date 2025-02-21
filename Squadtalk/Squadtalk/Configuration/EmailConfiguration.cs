namespace Squadtalk.Configuration;

public class EmailConfiguration
{
    public int Port { get; init; }
    public string Host { get; init; } = null!;
    public string Username { get; init; } = null!;
    public string Address { get; init; } = null!;
    public string Password { get; init; } = null!;
}
