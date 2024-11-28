using Shared.Data.Personalization;
using Shared.Services;
using Squadtalk.Client.Localization;
using TextLocalizer.Types;

namespace Squadtalk.Client.Services;

internal class BrowserTextProviderManager : ITextProviderManager
{
    private readonly Lazy<ILocalizedTextProvider> _defaultProvider = new(static () => CreateProvider(ApplicationLanguage.Default));
    private ProviderInfo? _currentProviderInfo;

    public ILocalizedTextProvider GetProvider(ApplicationLanguage language)
    {
        if (language == ApplicationLanguage.Default)
        {
            return _defaultProvider.Value;
        }

        if (_currentProviderInfo is { } providerInfo && providerInfo.Language == language)
        {
            return providerInfo.Provider;
        }

        var provider = CreateProvider(language);
        _currentProviderInfo = new ProviderInfo(provider, language);

        return provider;
    }

    private static ILocalizedTextProvider CreateProvider(ApplicationLanguage language) => language.Tag switch
    {
        ApplicationLanguage.EnglishTag => new EnglishTextProvider(),
        ApplicationLanguage.PolishTag => new PolishTextProvider(),
        _ => throw new ArgumentOutOfRangeException(nameof(language))
    };

    private readonly record struct ProviderInfo(ILocalizedTextProvider Provider, ApplicationLanguage Language);
}
