using System.Web;
using Microsoft.AspNetCore.Components;
using Shared.Enums;
using Shared.Services;
using Squadtalk.Client.Localization;

namespace Squadtalk.Services;

public class ServerLocalizationService : ILocalizationService
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly NavigationManager _navigationManager;

    public const string ContextLanguageItem = "UserLanguage";
    public const string LanguageCookieName = "UserLanguage";
    public const string LanguageQueryParam = "SetLang";

    private const string DefaultLanguage = "en-US";

    public event Action? LanguageChanged;

    public string UserLanguage { get; private set; }
    public SupportedLanguage SelectedLanguage { get; private set; }

    public ServerLocalizationService(
        IHttpContextAccessor contextAccessor,
        NavigationManager navigationManager)
    {
        _contextAccessor = contextAccessor;
        _navigationManager = navigationManager;

        var context = _contextAccessor.HttpContext;
        if (context?.Items.TryGetValue(ContextLanguageItem, out var userLanguage) is true
            && userLanguage is string language)
        {
            UserLanguage = language;
        }
        else
        {
            UserLanguage = DefaultLanguage;
        }

        SelectedLanguage = TextTable.ParseLanguageCode(UserLanguage);
    }

    public void SelectLanguage(SupportedLanguage language)
    {
        var languageCode = TextTable.GetLanguageCode(language);
        if (SelectedLanguage == language) return;

        UserLanguage = languageCode;
        SelectedLanguage = language;

        SaveLanguageSelection(languageCode);

        LanguageChanged?.Invoke();
    }

    private void SaveLanguageSelection(string languageCode)
    {
        var uri = new Uri(_navigationManager.Uri);

        var query = HttpUtility.ParseQueryString(uri.Query);
        query[LanguageQueryParam] = languageCode;

        var newUri = $"{uri.GetLeftPart(UriPartial.Path)}?{query}";
        _navigationManager.NavigateTo(newUri, forceLoad: true);
    }
}
