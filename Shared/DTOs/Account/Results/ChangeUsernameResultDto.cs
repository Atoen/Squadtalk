namespace Shared.DTOs.Account.Results;

public class ChangeUsernameResultDto
{
    public static ChangeUsernameResultDto NotChanged { get; } = new() { Type = ResultType.NotChanged };
    public static ChangeUsernameResultDto NotFound { get; } = new() { Type = ResultType.NotFound };
    public static ChangeUsernameResultDto UsernameInUse { get; } = new() { Type = ResultType.UsernameInUse };
    public static ChangeUsernameResultDto Unauthorized { get; } = new() { Type = ResultType.Unauthorized };
    public static ChangeUsernameResultDto FailedToChange { get; } = new() { Type = ResultType.FailedToChange };

    public static ChangeUsernameResultDto Success(string newUsername) => new()
    {
        Type = ResultType.Success,
        NewUsername = newUsername
    };

    public required ResultType Type { get; init; }
    public string? NewUsername { get; init; }

    public enum ResultType
    {
        Success = 1,
        NotChanged,
        NotFound,
        UsernameInUse,
        Unauthorized,
        FailedToChange
    }
}
