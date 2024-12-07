namespace Shared.Data.Results;

public sealed record LoginResult
{
    private LoginResult(int value) => Value = value;

    public int Value { get; }

    public static readonly LoginResult Success = new(1);
    public static readonly LoginResult Fail = new(2);
    public static readonly LoginResult NetworkError = new(3);
}
