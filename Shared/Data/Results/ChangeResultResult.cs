namespace Shared.Data.Results;

public abstract record ChangeEmailResult
{
    public sealed record ConfirmationSent : ChangeEmailResult;

    public sealed record NotChanged : ChangeEmailResult;

    public sealed record NotFound: ChangeEmailResult;

    public sealed record EmailInUse : ChangeEmailResult;

    public sealed record Unauthorized : ChangeEmailResult;

    public sealed record FailedToChange : ChangeEmailResult;

    public sealed record NetworkError : ChangeEmailResult;
}
