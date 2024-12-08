namespace Shared.Data.Results;

public abstract record ChangePasswordResult
{
    public sealed record Success : ChangePasswordResult;

    public sealed record Fail : ChangePasswordResult;

    public sealed record NetworkError : ChangePasswordResult;
}
