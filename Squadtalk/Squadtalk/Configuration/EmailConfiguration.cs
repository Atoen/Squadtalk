namespace Squadtalk.Configuration;

public class EmailConfiguration
{
    public int Port { get; set; }
    public string Host { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Password { get; set; } = null!;
}
