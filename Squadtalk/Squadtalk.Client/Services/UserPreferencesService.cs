using Microsoft.JSInterop;
using Shared.Data.Personalization;
using Shared.Services;

namespace Squadtalk.Client.Services;

public class UserPreferencesService : IUserPreferencesService
{
    private const string GetLanguageFunctionName = "getPreferredLanguage";
    private const string SaveLanguageFunctionName = "savePreferredLanguage";

    private const string GetThemeFunctionName = "getPreferredTheme";
    private const string SaveThemeFunctionName = "savePreferredTheme";

    private const string GetPaletteFunctionName = "getPreferredTheme";
    private const string SavePaletteFunctionName = "savePreferredTheme";

    private const string SaveAutoThemeFunctionName = "saveAutoTheme";

    private const string GetPreferencesFunctionName = "getPreferences";
    private const string SavePreferencesFunctionName = "savePreferences";

    private const string GetPrefersDarkModeFunctionName = "darkModeChange";

    private readonly IJSInProcessRuntime _jsRuntime;

    private readonly object[] _invokeArgs;
    private UserPreferences Preferences => (UserPreferences) _invokeArgs[0];

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

        var preferences = _jsRuntime.Invoke<UserPreferences>(GetPreferencesFunctionName);
        _invokeArgs = [preferences];

        logger.LogInformation("Oro: {Jajo}", preferences.Language);

        Language = ApplicationLanguage.ParseLanguageCode(preferences.Language);

        Theme = ApplicationTheme.ParseValue(preferences.Theme);
        UseDarkMode = ShouldUseDarkMode(Theme);

        Palette = ApplicationPalette.ParseValue(preferences.Palette);
    }

    public void ChangeLanguage(ApplicationLanguage language)
    {
        if (Language == language) return;

        Preferences.Language = language.Tag;
        SavePreferences();

        Language = language;

        LanguageChanged?.Invoke();
    }

    public void ChangeTheme(ApplicationTheme theme)
    {
        if (Theme == theme) return;

        Preferences.Theme = theme.Value;
        SavePreferences();

        Theme = theme;
        ThemeChanged?.Invoke();

        var useDarkMode = ShouldUseDarkMode(theme);
        if (UseDarkMode == useDarkMode) return;

        _jsRuntime.InvokeVoid(SaveAutoThemeFunctionName, useDarkMode);

        UseDarkMode = useDarkMode;
        UseDarkModeChanged?.Invoke();
    }

    public void ChangePalette(ApplicationPalette palette)
    {
        if (Palette == palette) return;

        Preferences.Palette = palette.Value;
        SavePreferences();

        Palette = palette;
        PaletteChanged?.Invoke();
    }

    private void SavePreferences()
    {
        _jsRuntime.InvokeVoid(SavePreferencesFunctionName, _invokeArgs);
    }

    private bool ShouldUseDarkMode(ApplicationTheme theme) => theme.Value switch
    {
        ApplicationTheme.LightValue => false,
        ApplicationTheme.DarkValue => true,
        _ => _jsRuntime.Invoke<bool>(GetPrefersDarkModeFunctionName)
    };
}

public class UserPreferences
{
    public required string Language { get; set; }
    public required string Theme { get; set; }
    public required string Palette { get; set; }
}
