namespace Shared.Data.Results;

public abstract record ChangeUsernameResult
{
    public sealed record Success(string NewUsername) : ChangeUsernameResult;

    public sealed record NotChanged : ChangeUsernameResult;

    public sealed record UsernameInUse : ChangeUsernameResult;

    public sealed record NotFound : ChangeUsernameResult;

    public sealed record Unauthorized : ChangeUsernameResult;

    public sealed record FailedToChange : ChangeUsernameResult;

    public sealed record NetworkError : ChangeUsernameResult;
}
