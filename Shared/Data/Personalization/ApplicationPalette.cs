namespace Shared.Data.Personalization;

public abstract record ApplicationPalette(string Value)
{
    private const string DefaultValue = "default";
    private const string OrangeValue = "orange";
    private const string BlueValue = "blue";
    private const string GreenValue = "green";

    public static readonly ApplicationPalette DefaultPalette = new Default();
    public static readonly ApplicationPalette OrangePalette = new Orange();
    public static readonly ApplicationPalette BluePalette = new Blue();
    public static readonly ApplicationPalette GreenPalette = new Green();

    public static ApplicationPalette ParseValue(ReadOnlySpan<char> value) => value switch
    {
        DefaultValue => DefaultPalette,
        OrangeValue => OrangePalette,
        BlueValue => BluePalette,
        GreenValue => GreenPalette,
        _ => DefaultPalette
    };

    public sealed record Default() : ApplicationPalette(DefaultValue);
    public sealed record Orange() : ApplicationPalette(OrangeValue);
    public sealed record Blue() : ApplicationPalette(BlueValue);
    public sealed record Green() : ApplicationPalette(GreenValue);
}