namespace Shared.Services;

public interface IFormValidator
{
    const int MinimumPasswordLength = 8;
    const int MaximumPasswordLength = 64;

    const int MaximumUsernameLength = 32;
    const int MinimumUsernameLength = 3;

    const int MaximumGroupNameLength = 100;

    const int MaximumTextMessageLength = 2000;

    const string AllowedUsernameChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    Func<string?, IEnumerable<string>> PasswordValidator { get; }
    Func<string?, string?> UsernameValidator { get; }
    Func<string?, string?> EmailValidator { get; }
    Func<string?, string?> GroupNameValidator { get; }

    string? PasswordMatches(string? first, string? second);

    IEnumerable<string> ValidatePassword(string? password);

    string? ValidateUsername(string? username);

    string? ValidateEmail(string? email);

    string? ValidateGroupName(string? groupName);
}
