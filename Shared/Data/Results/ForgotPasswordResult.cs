namespace Shared.Data.Results;

public abstract record ForgotPasswordResult
{
    public sealed record Success : ForgotPasswordResult;

    public sealed record Fail : ForgotPasswordResult;

    public sealed record NetworkError : ForgotPasswordResult;
}
