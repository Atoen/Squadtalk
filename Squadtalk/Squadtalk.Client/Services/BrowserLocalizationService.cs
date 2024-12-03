using Microsoft.JSInterop;
using Shared.Data.Personalization;

namespace Squadtalk.Client.Services;

public class BrowserLocalizationService
{
    private readonly IJSInProcessRuntime _jsRuntime;

    public event Action? LanguageChanged;

    public string UserLanguage { get; private set; }
    public ApplicationLanguage SelectedLanguage { get; private set; }

    public BrowserLocalizationService(IJSRuntime jsRuntime)
    {
        _jsRuntime = (IJSInProcessRuntime) jsRuntime;

        UserLanguage = _jsRuntime.Invoke<string>("getPreferredLanguage");
        // SelectedLanguage = TextTable.ParseLanguageCode(UserLanguage);
    }

    public void SelectLanguage(ApplicationLanguage language)
    {
        // var languageCode = TextTable.GetLanguageCode(language);
        // if (SelectedLanguage == language) return;
        //
        // UserLanguage = languageCode;
        // SelectedLanguage = language;
        //
        // SaveLanguageSelection(languageCode);
        //
        // LanguageChanged?.Invoke();
    }

    private void SaveLanguageSelection(string languageCode)
    {
        _jsRuntime.InvokeVoid("savePreferredLanguage", languageCode);
    }
}
