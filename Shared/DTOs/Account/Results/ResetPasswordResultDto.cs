namespace Shared.DTOs.Account.Results;

public class ResetPasswordResultDto
{
    public static ResetPasswordResultDto Success { get; } = new() { Successful = true };
    public static ResetPasswordResultDto Fail { get; } = new() { Successful = false };

    public required bool Successful { get; init; }
}
