using MudBlazor;
using MudBlazor.Utilities;

namespace Squadtalk.Client.Palettes;

public class OrangeLightPalette : PaletteLight
{
    public override MudColor Primary { get; set; } = Colors.Orange.Default; // Orange as the primary color
    public override MudColor PrimaryContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Secondary { get; set; } = Colors.Pink.Accent2; // Pink as the secondary color
    public override MudColor SecondaryContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Tertiary { get; set; } = "#1EC8A5"; // Light green
    public override MudColor TertiaryContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Success { get; set; } = Colors.Green.Accent4;
    public override MudColor SuccessContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Warning { get; set; } = Colors.Orange.Lighten1; // Lighter orange for warning
    public override MudColor WarningContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Error { get; set; } = Colors.Red.Default;
    public override MudColor ErrorContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Dark { get; set; } = Colors.Gray.Darken3;
    public override MudColor DarkContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Background { get; set; } = Colors.Shades.White;
}

public class OrangeDarkPalette : PaletteDark
{
    public override MudColor Primary { get; set; } = Colors.Orange.Darken2; // Darker orange for dark mode
    public override MudColor PrimaryContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Secondary { get; set; } = Colors.Pink.Darken2; // Darker pink for secondary color
    public override MudColor SecondaryContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Tertiary { get; set; } = "#1EC8A5"; // Keep as is or adjust
    public override MudColor TertiaryContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Success { get; set; } = Colors.Green.Accent4;
    public override MudColor SuccessContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Warning { get; set; } = Colors.Orange.Darken1; // Darker warning color
    public override MudColor WarningContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Error { get; set; } = Colors.Red.Accent4;
    public override MudColor ErrorContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Dark { get; set; } = Colors.Gray.Darken4; // Darker background for dark mode
    public override MudColor DarkContrastText { get; set; } = Colors.Shades.White;
    public override MudColor Background { get; set; } = Colors.Gray.Darken3;
}


