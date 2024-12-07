using System.Text.RegularExpressions;
using Squadtalk.Client.Localization;

namespace Squadtalk.Client.Services;

public partial class PasswordValidator
{
    private readonly LocalizedText _localizedText;

    public readonly Func<string, IEnumerable<string>> StrengthValidator;

    public PasswordValidator(LocalizedText localizedText)
    {
        _localizedText = localizedText;
        StrengthValidator = ValidateStrength;
    }

    public IEnumerable<string> ValidateStrength(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            yield return _localizedText.R.password_is_required;
            yield break;
        }

        if (password.Length < 8)
        {
            yield return _localizedText.R.password_length;
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

    [GeneratedRegex("[A-Z]")]
    private static partial Regex UppercaseRegex();

    [GeneratedRegex("[a-z]")]
    private static partial Regex LowercaseRegex();

    [GeneratedRegex("[0-9]")]
    private static partial Regex DigitRegex();
}
