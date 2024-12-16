using System.Text.RegularExpressions;
using Shared.Extensions;
using Shared.Services;
using Squadtalk.Client.Localization;

namespace Squadtalk.Client.Services;

internal partial class FormValidator : IFormValidator
{
    private readonly LocalizedText _localizedText;

    public Func<string?, IEnumerable<string>> PasswordValidator { get; }
    public Func<string?, string?> UsernameValidator { get; }
    public Func<string?, string?> EmailValidator { get; }

    public FormValidator(LocalizedText localizedText)
    {
        _localizedText = localizedText;

        PasswordValidator = ValidatePassword;
        UsernameValidator = ValidateUsername;
        EmailValidator = ValidateEmail;
    }

    public string? PasswordMatches(string? first, string? second)
    {
        var matches = first == second;
        return matches ? null : _localizedText.R.passwords_dont_match;
    }

    public IEnumerable<string> ValidatePassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            yield return _localizedText.R.password_is_required;
            yield break;
        }

        if (password.Length < IFormValidator.MinimumPasswordLength)
        {
            yield return _localizedText.R.password_too_short.Format(IFormValidator.MinimumPasswordLength);
        }

        if (password.Length > IFormValidator.MaximumPasswordLength)
        {
            yield return _localizedText.R.password_too_long.Format(IFormValidator.MaximumPasswordLength);
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
        if (string.IsNullOrWhiteSpace(username))
        {
            return _localizedText.R.username_is_required;
        }

        if (username.Length < IFormValidator.MinimumUsernameLength)
        {
            return _localizedText.R.username_too_short;
        }

        if (username.Length > IFormValidator.MaximumUsernameLength)
        {
            return _localizedText.R.username_too_long;
        }

        if (!AllowedUsernameRegex().IsMatch(username))
        {
            return _localizedText.R.username_invalid_character;
        }

        return null;
    }

    public string? ValidateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return _localizedText.R.email_is_required;
        }

        var span = email.AsSpan();

        if (span.ContainsAny('\r', '\n'))
        {
            return _localizedText.R.email_is_invalid;
        }

        var atIndex = span.IndexOf('@');

        var isValid = atIndex > 0 &&
                      atIndex != span.Length - 1 &&
                      atIndex == span.LastIndexOf('@');

        return isValid ? null : _localizedText.R.email_is_invalid;
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
