using Shared.Enums;

namespace Shared.Services;

public interface ILocalizationService
{
    string UserLanguage { get; }

    SupportedLanguage SelectedLanguage { get; }

    void SelectLanguage(SupportedLanguage language);

    event Action? LanguageChanged;
}
