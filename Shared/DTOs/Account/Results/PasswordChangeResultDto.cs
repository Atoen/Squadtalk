namespace Shared.DTOs.Account.Results;

public class PasswordChangeResultDto
{
    public static PasswordChangeResultDto Success { get; } = new() { Successful = true };
    public static PasswordChangeResultDto Fail { get; } = new() { Successful = false };

    public required bool Successful { get; init; }
}
