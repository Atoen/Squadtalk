using Microsoft.JSInterop;
using Shared.Data.Personalization;
using Shared.Services;

namespace Squadtalk.Client.Services;

public class UserPreferencesService : IUserPreferencesService
{
    private const string GetPreferencesFunctionName = "getPreferences";
    private const string SavePreferencesFunctionName = "savePreferences";

    private const string GetPrefersDarkModeFunctionName = "darkModeChange";

    private readonly IJSInProcessRuntime _jsRuntime;
    private readonly object?[] _invokeArgs;

    public ApplicationLanguage Language { get; private set; }
    public ApplicationTheme Theme { get; private set; }
    public ApplicationPalette Palette { get; private set; }
    public bool UseDarkMode { get; private set; }

    public event Action? LanguageChanged;
    public event Action? ThemeChanged;
    public event Action? UseDarkModeChanged;
    public event Action? PaletteChanged;

    public UserPreferencesService(IJSRuntime jsRuntime, ILogger<UserPreferencesService> logger)
    {
        _jsRuntime = (IJSInProcessRuntime) jsRuntime;

        var preferencesData = _jsRuntime.Invoke<string?>(GetPreferencesFunctionName);
        _invokeArgs = [preferencesData];

        logger.LogInformation("Retrieved preferences data: {Data}", preferencesData);

        var preferences = ApplicationPreferences.Parse(preferencesData);

        Language = preferences.Language;
        Palette = preferences.Palette;
        Theme = preferences.Theme;
        UseDarkMode = ShouldUseDarkMode(Theme);
    }

    public void ChangeLanguage(ApplicationLanguage language)
    {
        if (Language == language) return;

        Language = language;
        SavePreferences();

        LanguageChanged?.Invoke();
    }

    public void ChangeTheme(ApplicationTheme theme)
    {
        if (Theme == theme) return;

        Theme = theme;
        ThemeChanged?.Invoke();

        var useDarkMode = ShouldUseDarkMode(theme);
        if (UseDarkMode != useDarkMode)
        {
            UseDarkMode = useDarkMode;
            UseDarkModeChanged?.Invoke();
        }

        SavePreferences();
    }

    public void ChangePalette(ApplicationPalette palette)
    {
        if (Palette == palette) return;

        Palette = palette;
        SavePreferences();

        PaletteChanged?.Invoke();
    }

    private void SavePreferences()
    {
        var darkMode = UseDarkMode ? ApplicationTheme.AutoMode.Dark : ApplicationTheme.AutoMode.Light;
        var preferences = new ApplicationPreferences(Language, Theme, Palette, darkMode);

        _invokeArgs[0] = preferences.Serialize();
        _jsRuntime.InvokeVoid(SavePreferencesFunctionName, _invokeArgs);
    }

    private bool ShouldUseDarkMode(ApplicationTheme theme) => theme.Value switch
    {
        ApplicationTheme.LightValue => false,
        ApplicationTheme.DarkValue => true,
        _ => _jsRuntime.Invoke<bool>(GetPrefersDarkModeFunctionName)
    };
}
