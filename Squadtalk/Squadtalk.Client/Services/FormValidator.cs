using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Shared.Extensions;
using Squadtalk.Client.Localization;

namespace Squadtalk.Client.Services;

public partial class FormValidator
{
    public const int MinimumPasswordLength = 8;
    public const int MaximumPasswordLength = 64;

    public const int MaximumUsernameLength = 32;
    public const int MinimumUsernameLength = 3;

    public const string AllowedUsernameChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    private readonly LocalizedText _localizedText;

    public readonly Func<string?, IEnumerable<string>> PasswordValidator;
    public readonly Func<string?, string?> UsernameValidator;
    public readonly EmailAddressAttribute EmailValidator;

    public FormValidator(LocalizedText localizedText)
    {
        _localizedText = localizedText;

        PasswordValidator = ValidatePassword;
        UsernameValidator = ValidateUsername;
        EmailValidator = new EmailAddressAttribute
        {
            ErrorMessage = _localizedText.R.email_is_invalid
        };
    }

    public IEnumerable<string> ValidatePassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            yield return _localizedText.R.password_is_required;
            yield break;
        }

        if (password.Length < MinimumPasswordLength)
        {
            yield return _localizedText.R.password_too_short.Format(MinimumPasswordLength);
        }

        if (password.Length > MaximumPasswordLength)
        {
            yield return _localizedText.R.password_too_long.Format(MaximumPasswordLength);
        }

        if (!UppercaseRegex().IsMatch(password))
        {
            yield return _localizedText.R.password_uppercase;
        }

        if (!LowercaseRegex().IsMatch(password))
        {
            yield return _localizedText.R.password_lowercase;
        }

        if (!DigitRegex().IsMatch(password))
        {
            yield return _localizedText.R.password_digit;
        }
    }

    public string? ValidateUsername(string? username)
    {
        if (username is null)
        {
            return _localizedText.R.username_is_required;
        }

        if (username.Length < MinimumUsernameLength)
        {
            return _localizedText.R.username_too_short;
        }

        if (username.Length > MaximumUsernameLength)
        {
            return _localizedText.R.username_too_long;
        }

        if (!AllowedUsernameRegex().IsMatch(username))
        {
            return _localizedText.R.username_invalid_character;
        }

        return null;
    }

    [GeneratedRegex("[A-Z]")]
    private static partial Regex UppercaseRegex();

    [GeneratedRegex("[a-z]")]
    private static partial Regex LowercaseRegex();

    [GeneratedRegex("[0-9]")]
    private static partial Regex DigitRegex();

    [GeneratedRegex("^[a-zA-Z0-9-._@+]*$")]
    private static partial Regex AllowedUsernameRegex();
}
