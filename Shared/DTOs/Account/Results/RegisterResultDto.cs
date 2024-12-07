namespace Shared.DTOs.Account.Results;

public class RegisterResultDto
{
    public static RegisterResultDto Success { get; } = new() { Type = ResultType.Success };
    public static RegisterResultDto EmailInUse { get; } = new() { Type = ResultType.EmailInUse };
    public static RegisterResultDto UsernameInUse { get; } = new() { Type = ResultType.UsernameInUse };
    public static RegisterResultDto FailedToCreateAccount { get; } = new() { Type = ResultType.FailedToCreateAccount };

    public required ResultType Type { get; init; }

    public enum ResultType
    {
        Success = 1,
        EmailInUse,
        UsernameInUse,
        FailedToCreateAccount,
    }
}
