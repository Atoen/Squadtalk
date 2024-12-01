namespace Shared.Data.Personalization;

public sealed record ApplicationTheme
{
    public const string AutoValue = "auto";
    public const string LightValue = "light";
    public const string DarkValue = "dark";

    public const string AutoLightValue = "auto-light";
    public const string AutoDarkValue = "auto-dark";

    public static readonly ApplicationTheme Auto = new(AutoValue);
    public static readonly ApplicationTheme Light = new(LightValue);
    public static readonly ApplicationTheme Dark = new(DarkValue);

    public static readonly ApplicationTheme Default = Light;

    public string Value { get; }

    private ApplicationTheme(string value) => Value = value;

    public static ApplicationTheme ParseValue(ReadOnlySpan<char> value) => value switch
    {
        AutoValue => Auto,
        LightValue => Light,
        DarkValue => Dark,
        _ => Default
    };

    public enum AutoMode
    {
        Light,
        Dark
    }
}

public static class ApplicationThemeExtensions
{
    public static string Value(this ApplicationTheme.AutoMode autoMode) => autoMode switch
    {
        ApplicationTheme.AutoMode.Light => ApplicationTheme.AutoLightValue,
        ApplicationTheme.AutoMode.Dark => ApplicationTheme.AutoDarkValue,
        _ => string.Empty
    };

    public static ApplicationTheme.AutoMode ParseAutoModeValue(ReadOnlySpan<char> value) => value switch
    {
        ApplicationTheme.AutoLightValue => ApplicationTheme.AutoMode.Light,
        ApplicationTheme.AutoDarkValue => ApplicationTheme.AutoMode.Dark,
        _ => default
    };
}
