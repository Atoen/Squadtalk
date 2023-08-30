namespace Squadtalk.Client.Options;

public class JwtServiceOptions
{
    public int RetryAttempts { get; set; } = 1;
    public int[] RetryDelays { get; set; } = { 0 };
}