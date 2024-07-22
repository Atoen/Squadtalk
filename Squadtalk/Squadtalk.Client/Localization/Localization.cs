using Microsoft.AspNetCore.Components;
using Shared.Data;
using Shared.Services;
using Squadtalk.Client.Extensions;

namespace Squadtalk.Client.Localization;

public sealed class Localization : ILocalization, IDisposable
{
    private readonly ILocalizationService _localizationService;

    private readonly WeakRefCollection<ComponentBase> _components = new();

    public Localization(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
        _localizationService.LanguageChanged += LanguageChanged;

        LanguageChanged();
    }

    public TextTable TextTable { get; } = new();

    public TextTable GetTextTable(ComponentBase component)
    {
        _components.Add(component);
        return TextTable;
    }

    private void LanguageChanged()
    {
        TextTable.SetLanguage(_localizationService.SelectedLanguage);
        _components.InvokeStateHasChanged();
    }

    public void Dispose()
    {
        _localizationService.LanguageChanged -= LanguageChanged;
    }
}
