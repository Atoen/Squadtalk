namespace Shared.Data.Personalization;

public sealed record ApplicationPalette
{
    public const string DefaultValue = "default";
    public const string OrangeValue = "orange";
    public const string BlueValue = "blue";
    public const string GreenValue = "green";

    public static readonly ApplicationPalette Default = new(DefaultValue);
    public static readonly ApplicationPalette Orange = new(OrangeValue);
    public static readonly ApplicationPalette Blue = new(BlueValue);
    public static readonly ApplicationPalette Green = new(GreenValue);

    public string Value { get; }

    private ApplicationPalette(string value) => Value = value;

    public static ApplicationPalette ParseValue(ReadOnlySpan<char> value) => value switch
    {
        DefaultValue => Default,
        OrangeValue => Orange,
        BlueValue => Blue,
        GreenValue => Green,
        _ => Default
    };
}
