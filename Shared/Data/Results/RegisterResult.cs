namespace Shared.Data.Results;

public sealed record RegisterResult
{
    private RegisterResult(int value) => Value = value;

    public int Value { get; }

    public static readonly RegisterResult Success = new(1);
    public static readonly RegisterResult EmailInUse = new(2);
    public static readonly RegisterResult UsernameInUse = new(3);
    public static readonly RegisterResult FailedToCreateAccount = new(4);
    public static readonly RegisterResult NetworkError = new(5);

}
