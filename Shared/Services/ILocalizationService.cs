using Shared.Data.Personalization;

namespace Shared.Services;

public interface ILocalizationService
{
    string UserLanguage { get; }

    ApplicationLanguage SelectedLanguage { get; }

    void SelectLanguage(ApplicationLanguage language);

    event Action? LanguageChanged;
}
