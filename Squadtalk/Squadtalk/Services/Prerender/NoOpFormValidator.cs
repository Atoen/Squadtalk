using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal class NoOpFormValidator : IFormValidator
{
    private static readonly Func<string?, IEnumerable<string>> Enumerable = _ => [];
    private static readonly Func<string?, string?> String = _ => null;

    public Func<string?, IEnumerable<string>> PasswordValidator => Enumerable;
    public Func<string?, string?> UsernameValidator => String;
    public Func<string?, string?> EmailValidator => String;

    public string? PasswordMatches(string? first, string? second) => null;

    public IEnumerable<string> ValidatePassword(string? password) => [];

    public string? ValidateUsername(string? username) => null;

    public string? ValidateEmail(string? email) => null;
}
