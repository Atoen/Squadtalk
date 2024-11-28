using Shared.Data.Personalization;

namespace Shared.Services;

public interface IUserPreferencesService
{
    ApplicationLanguage Language { get; }

    ApplicationTheme Theme { get; }

    event Action? LanguageChanged;

    event Action? ThemeChanged;

    void ChangeLanguage(ApplicationLanguage language);

    void ChangeTheme(ApplicationTheme theme);
}
