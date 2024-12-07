namespace Shared.DTOs.Account.Results;

public class LoginResultDto
{
    public static LoginResultDto Success { get; } = new() { Successful = true };
    public static LoginResultDto Fail { get; } = new() { Successful = false };

    public required bool Successful { get; init; }
}