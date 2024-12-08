namespace Shared.DTOs.Account.Results;

public class ChangePasswordResultDto
{
    public static ChangePasswordResultDto Success { get; } = new() { Successful = true };
    public static ChangePasswordResultDto Fail { get; } = new() { Successful = false };

    public required bool Successful { get; init; }
}
