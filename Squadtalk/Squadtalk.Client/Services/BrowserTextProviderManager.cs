using Shared.Data.Personalization;
using Shared.Services;
using Squadtalk.Client.Localization;
using TextLocalizer.Types;

namespace Squadtalk.Client.Services;

internal class BrowserTextProviderManager : ITextProviderManager
{
    private ILocalizedTextProvider? _defaultProvider;
    private ProviderInfo? _currentProviderInfo;

    private ILocalizedTextProvider DefaultProvider => _defaultProvider ??= CreateProvider(ApplicationLanguage.DefaultLanguage);

    public ILocalizedTextProvider GetProvider(ApplicationLanguage language)
    {
        if (language == ApplicationLanguage.DefaultLanguage)
        {
            return DefaultProvider;
        }

        if (_currentProviderInfo is { } providerInfo && providerInfo.Language == language)
        {
            return providerInfo.Provider;
        }

        var provider = CreateProvider(language);
        _currentProviderInfo = new ProviderInfo(provider, language);

        return provider;
    }

    private static ILocalizedTextProvider CreateProvider(ApplicationLanguage language) => language switch
    {
        ApplicationLanguage.English => new EnglishTextProvider(),
        ApplicationLanguage.Polish => new PolishTextProvider(),
        _ => throw new ArgumentOutOfRangeException(nameof(language))
    };

    private readonly record struct ProviderInfo(ILocalizedTextProvider Provider, ApplicationLanguage Language);
}
