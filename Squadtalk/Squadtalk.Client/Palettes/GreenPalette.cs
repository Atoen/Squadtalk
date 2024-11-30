using MudBlazor;
using MudBlazor.Utilities;

namespace Squadtalk.Client.Palettes;

public class GreenLightPalette : PaletteLight
{
    public override MudColor Primary { get; set; } = Colors.Green.Accent4; // Green as primary color
    public override MudColor PrimaryContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Secondary { get; set; } = Colors.Pink.Accent2;
    public override MudColor SecondaryContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Tertiary { get; set; } = "#1EC8A5"; // Light green tertiary color
    public override MudColor TertiaryContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Success { get; set; } = Colors.Green.Accent2;
    public override MudColor SuccessContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Warning { get; set; } = Colors.Orange.Default;
    public override MudColor WarningContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Error { get; set; } = Colors.Red.Default;
    public override MudColor ErrorContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Dark { get; set; } = Colors.Gray.Darken3;
    public override MudColor DarkContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Background { get; set; } = Colors.Shades.White;
}

public class GreenDarkPalette : PaletteDark
{
    public override MudColor Primary { get; set; } = Colors.Green.Darken1; // Darker green for dark mode
    public override MudColor PrimaryContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Secondary { get; set; } = Colors.Pink.Darken2;
    public override MudColor SecondaryContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Tertiary { get; set; } = "#1EC8A5"; // Adjust if needed
    public override MudColor TertiaryContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Success { get; set; } = Colors.Green.Accent3;
    public override MudColor SuccessContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Warning { get; set; } = Colors.Orange.Darken1;
    public override MudColor WarningContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Error { get; set; } = Colors.Red.Accent4;
    public override MudColor ErrorContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Dark { get; set; } = Colors.Gray.Darken4; // Darker background
    public override MudColor DarkContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Background { get; set; } = Colors.Gray.Darken4;
}

