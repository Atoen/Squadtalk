namespace Shared.Data.Results;

public abstract record LoginResult
{
    public sealed record Success : LoginResult;

    public sealed record Fail : LoginResult;

    public sealed record NetworkError : LoginResult;
}