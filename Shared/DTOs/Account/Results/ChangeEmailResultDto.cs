namespace Shared.DTOs.Account.Results;

public class ChangeEmailResultDto
{
    public static ChangeEmailResultDto ConfirmationSent { get; } = new() { Type = ResultType.ConfirmationSent };
    public static ChangeEmailResultDto NotChanged { get; } = new() { Type = ResultType.NotChanged };
    public static ChangeEmailResultDto NotFound { get; } = new() { Type = ResultType.NotFound };
    public static ChangeEmailResultDto EmailInUse { get; } = new() { Type = ResultType.EmailInUse };
    public static ChangeEmailResultDto Unauthorized { get; } = new() { Type = ResultType.Unauthorized };

    public required ResultType Type { get; init; }

    public enum ResultType
    {
        ConfirmationSent = 1,
        NotChanged,
        NotFound,
        EmailInUse,
        Unauthorized
    }
}
