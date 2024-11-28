using Shared.Data.Personalization;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

public class ServerUserPreferencesService : IUserPreferencesService
{
    private readonly IHttpContextAccessor _contextAccessor;

    public ApplicationLanguage Language { get; private set; }
    public ApplicationTheme Theme { get; private set; }

    public event Action? LanguageChanged;
    public event Action? ThemeChanged;

    public ServerUserPreferencesService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;

        Language = GetContextValue(LocalizationMiddleware.ContextLanguageItem) is { } language
            ? ApplicationLanguage.ParseLanguageCode(language)
            : ApplicationLanguage.Default;

        Theme = GetContextValue(LocalizationMiddleware.ContextThemeItem) is { } theme
            ? ApplicationTheme.ParseValue(theme)
            : ApplicationTheme.Default;
    }

    public void ChangeLanguage(ApplicationLanguage language) => Language = language;

    public void ChangeTheme(ApplicationTheme theme) => Theme = theme;

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
