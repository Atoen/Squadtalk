using Shared.Data.Personalization;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

public class ServerUserPreferencesService : IUserPreferencesService
{
    private readonly IHttpContextAccessor _contextAccessor;

    public ApplicationLanguage Language { get; }
    public ApplicationTheme Theme { get; }
    public ApplicationPalette Palette { get; }
    public bool UseDarkMode { get; }

    public event Action? LanguageChanged;
    public event Action? ThemeChanged;
    public event Action? UseDarkModeChanged;
    public event Action? PaletteChanged;

    public ServerUserPreferencesService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;

        var data = GetContextValue(UserPreferencesMiddleware.ContextPreferencesItem);
        var preferences = ApplicationPreferences.Parse(data);

        Language = preferences.Language;
        Theme = preferences.Theme;
        Palette = preferences.Palette;

        UseDarkMode = ShouldUseDarkMode(Theme, preferences.AutoMode);
    }

    public void ChangeLanguage(ApplicationLanguage language) {}

    public void ChangeTheme(ApplicationTheme theme) {}

    public void ChangePalette(ApplicationPalette palette) {}

    private bool ShouldUseDarkMode(ApplicationTheme theme, ApplicationTheme.Automatic.Mode autoMode)
    {
        return theme switch
        {
            ApplicationTheme.Light => false,
            ApplicationTheme.Dark => true,
            _ => autoMode == ApplicationTheme.Automatic.Mode.Dark
        };

    }

    private string? GetContextValue(string name)
    {
        var context = _contextAccessor.HttpContext;
        if (context is null)
        {
            return null;
        }

        if (context.Items.TryGetValue(name, out var value) && value is string stringValue)
        {
            return stringValue;
        }

        return null;
    }
}
