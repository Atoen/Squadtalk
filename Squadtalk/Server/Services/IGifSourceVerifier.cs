namespace Squadtalk.Server.Services;

public interface IGifSourceVerifier
{
    ValueTask<bool> VerifyAsync(string source);
}