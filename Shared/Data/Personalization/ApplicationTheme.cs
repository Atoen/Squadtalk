namespace Shared.Data.Personalization;

public abstract record ApplicationTheme(string Value)
{
    private const string LightValue = "light";
    private const string DarkValue = "dark";
    private const string AutoValue = "auto";

    public static readonly ApplicationTheme LightTheme = new Light();
    public static readonly ApplicationTheme DarkTheme = new Dark();
    public static readonly ApplicationTheme AutoTheme = new Automatic();

    public static readonly ApplicationTheme DefaultTheme = LightTheme;

    public static ApplicationTheme ParseValue(ReadOnlySpan<char> value) => value switch
    {
        LightValue => LightTheme,
        DarkValue => DarkTheme,
        AutoValue => AutoTheme,
        _ => DefaultTheme
    };

    public sealed record Light() : ApplicationTheme(LightValue);

    public sealed record Dark() : ApplicationTheme(DarkValue);

    public sealed record Automatic() : ApplicationTheme(AutoValue)
    {
        internal const string AutoLightValue = "auto-light";
        internal const string AutoDarkValue = "auto-dark";

        public enum Mode
        {
            Light,
            Dark
        }
    }
}

public static class ApplicationThemeExtensions
{
    public static string Value(this ApplicationTheme.Automatic.Mode autoMode) => autoMode switch
    {
        ApplicationTheme.Automatic.Mode.Light => ApplicationTheme.Automatic.AutoLightValue,
        ApplicationTheme.Automatic.Mode.Dark => ApplicationTheme.Automatic.AutoDarkValue,
        _ => string.Empty
    };

    public static ApplicationTheme.Automatic.Mode ParseAutoModeValue(ReadOnlySpan<char> value) => value switch
    {
        ApplicationTheme.Automatic.AutoLightValue => ApplicationTheme.Automatic.Mode.Light,
        ApplicationTheme.Automatic.AutoDarkValue => ApplicationTheme.Automatic.Mode.Dark,
        _ => default
    };
}
