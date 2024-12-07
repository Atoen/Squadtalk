namespace Shared.DTOs.Account.Results;

public class EmailChangeResultDto
{
    public static EmailChangeResultDto ConfirmationSent { get; } = new() { Type = ResultType.ConfirmationSent };
    public static EmailChangeResultDto NotChanged { get; } = new() { Type = ResultType.NotChanged };
    public static EmailChangeResultDto NotFound { get; } = new() { Type = ResultType.NotFound };
    public static EmailChangeResultDto EmailInUse { get; } = new() { Type = ResultType.EmailInUse };
    public static EmailChangeResultDto Unauthorized { get; } = new() { Type = ResultType.Unauthorized };

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
