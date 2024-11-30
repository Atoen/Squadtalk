using MudBlazor;
using MudBlazor.Utilities;

namespace Squadtalk.Client.Palettes;

public class BlueLightPalette : PaletteLight
{
    public override MudColor Primary { get; set; } = Colors.Blue.Default; // Blue as primary color
    public override MudColor PrimaryContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Secondary { get; set; } = Colors.Cyan.Accent2; // Cyan as secondary color
    public override MudColor SecondaryContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Tertiary { get; set; } = "#1EC8A5"; // Light green
    public override MudColor TertiaryContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Success { get; set; } = Colors.Green.Accent4;
    public override MudColor SuccessContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Warning { get; set; } = Colors.Orange.Default;
    public override MudColor WarningContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Error { get; set; } = Colors.Red.Default;
    public override MudColor ErrorContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Dark { get; set; } = Colors.Gray.Darken3;
    public override MudColor DarkContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Background { get; set; } = Colors.Shades.White;
}

public class BlueDarkPalette : PaletteDark
{
    public override MudColor Primary { get; set; } = Colors.Blue.Darken2; // Darker blue for dark mode
    public override MudColor PrimaryContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Secondary { get; set; } = Colors.Cyan.Darken1; // Darker cyan for secondary color
    public override MudColor SecondaryContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Tertiary { get; set; } = "#1EC8A5"; // Same light green
    public override MudColor TertiaryContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Success { get; set; } = Colors.Green.Accent4;
    public override MudColor SuccessContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Warning { get; set; } = Colors.Orange.Darken1;
    public override MudColor WarningContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Error { get; set; } = Colors.Red.Accent4;
    public override MudColor ErrorContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Dark { get; set; } = Colors.Gray.Darken4; // Darker background for dark mode
    public override MudColor DarkContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Background { get; set; } = Colors.Gray.Darken4;
}
