namespace Shared.Data.Results;

public abstract record ResetPasswordResult
{
    public sealed record Success : ResetPasswordResult;

    public sealed record Fail : ResetPasswordResult;

    public sealed record NetworkError : ResetPasswordResult;
}
