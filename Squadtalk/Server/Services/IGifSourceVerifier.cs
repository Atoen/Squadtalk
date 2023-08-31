namespace Squadtalk.Server.Services;

public interface IGifSourceVerifier
{
    public ValueTask<bool> VerifyAsync(string source);
}