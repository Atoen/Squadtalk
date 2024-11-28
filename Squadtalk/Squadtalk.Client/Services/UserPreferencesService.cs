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

    private readonly IJSInProcessRuntime _jsRuntime;

    public ApplicationLanguage Language { get; set; }

    public ApplicationTheme Theme { get; set; }

    public event Action? LanguageChanged;

    public event Action? ThemeChanged;

    public UserPreferencesService(IJSRuntime jsRuntime)
    {
        _jsRuntime = (IJSInProcessRuntime) jsRuntime;

        var userLanguage = _jsRuntime.Invoke<string>(GetLanguageFunctionName);
        Language = ApplicationLanguage.ParseLanguageCode(userLanguage);

        var userTheme = _jsRuntime.Invoke<string>(GetThemeFunctionName);
        Theme = ApplicationTheme.ParseValue(userTheme);
    }

    public void ChangeLanguage(ApplicationLanguage language)
    {
        if (Language == language) return;

        _jsRuntime.InvokeVoid(SaveLanguageFunctionName, language.Tag);
        Language = language;

        LanguageChanged?.Invoke();
    }

    public void ChangeTheme(ApplicationTheme theme)
    {
        if (Theme == theme) return;

        _jsRuntime.InvokeVoid(SaveThemeFunctionName, theme.Value);
        Theme = theme;

        ThemeChanged?.Invoke();
    }
}
