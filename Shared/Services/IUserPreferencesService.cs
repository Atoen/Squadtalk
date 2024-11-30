using Shared.Data.Personalization;

namespace Shared.Services;

public interface IUserPreferencesService
{
    ApplicationLanguage Language { get; }

    ApplicationTheme Theme { get; }

    ApplicationPalette Palette { get; }

    bool UseDarkMode { get; }

    event Action? LanguageChanged;

    event Action? ThemeChanged;

    event Action? UseDarkModeChanged;

    event Action? PaletteChanged;

    void ChangeLanguage(ApplicationLanguage language);

    void ChangeTheme(ApplicationTheme theme);

    void ChangePalette(ApplicationPalette palette);
}
