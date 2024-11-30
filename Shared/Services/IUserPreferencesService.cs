using Shared.Data.Personalization;

namespace Shared.Services;

public interface IUserPreferencesService
{
    ApplicationLanguage Language { get; }

    ApplicationTheme Theme { get; }

    bool UseDarkMode { get; }

    event Action? LanguageChanged;

    event Action? ThemeChanged;

    event Action? UseDarkModeChanged;

    void ChangeLanguage(ApplicationLanguage language);

    void ChangeTheme(ApplicationTheme theme);
}
