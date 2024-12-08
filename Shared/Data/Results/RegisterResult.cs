namespace Shared.Data.Results;

public abstract record RegisterResult
{
    public sealed record Success : RegisterResult;

    public sealed record EmailInUse : RegisterResult;

    public sealed record UsernameInUse : RegisterResult;

    public sealed record FailedToCreateAccount : RegisterResult;

    public sealed record NetworkError : RegisterResult;
}