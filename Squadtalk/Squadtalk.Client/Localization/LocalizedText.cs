using Microsoft.AspNetCore.Components;
using Shared.Data;
using Shared.Data.Personalization;
using Shared.Services;
using Squadtalk.Client.Extensions;
using TextLocalizer;
using TextLocalizer.Types;

namespace Squadtalk.Client.Localization;

[LocalizationTable(
    CurrentProviderAccessor = nameof(Provider),
    DefaultProviderAccessor = nameof(DefaultProvider),
    TableName = "R",
    GenerateDocs = true
)]
public sealed partial class LocalizedText : IDisposable
{
    private readonly IUserPreferencesService _userPreferencesService;
    private readonly ITextProviderManager _textProviderManager;

    private readonly WeakRefCollection<ComponentBase> _components = new();

    public LocalizedText(IUserPreferencesService userPreferencesService, ITextProviderManager textProviderManager)
    {
        _userPreferencesService = userPreferencesService;
        _textProviderManager = textProviderManager;

        _userPreferencesService.LanguageChanged += LanguageChanged;
    }

    private ILocalizedTextProvider Provider => _textProviderManager.GetProvider(_userPreferencesService.Language);
    private ILocalizedTextProvider DefaultProvider => _textProviderManager.GetProvider(ApplicationLanguage.DefaultLanguage);

    public TextTable GetTextTable(ComponentBase component)
    {
        if (OperatingSystem.IsBrowser())
        {
            _components.Add(component);
        }

        return R;
    }

    private void LanguageChanged() => _components.InvokeStateHasChanged();

    public void Dispose()
    {
        _userPreferencesService.LanguageChanged -= LanguageChanged;
    }
}