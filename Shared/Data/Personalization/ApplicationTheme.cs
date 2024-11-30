namespace Shared.Data.Personalization;

public sealed record ApplicationTheme
{
    public const string AutoValue = "auto";
    public const string LightValue = "light";
    public const string DarkValue = "dark";

    public static readonly ApplicationTheme Auto = new(AutoValue);
    public static readonly ApplicationTheme Light = new(LightValue);
    public static readonly ApplicationTheme Dark = new(DarkValue);

    public static readonly ApplicationTheme Default = Light;

    public string Value { get; }

    private ApplicationTheme(string value) => Value = value;

    public static ApplicationTheme ParseValue(string value) => value switch
    {
        AutoValue => Auto,
        LightValue => Light,
        DarkValue => Dark,
        _ => Default
    };
}
