using System.Collections.Concurrent;
using Shared.Data.Personalization;
using Shared.Services;
using Squadtalk.Client.Localization;
using TextLocalizer.Types;

namespace Squadtalk.Services.Prerender;

internal class ServerTextProviderManager : ITextProviderManager
{
    private readonly ConcurrentDictionary<ApplicationLanguage, ILocalizedTextProvider> _providers = new();

    public ILocalizedTextProvider GetProvider(ApplicationLanguage language)
    {
        return _providers.GetOrAdd(language, CreateProvider);
    }

    private static ILocalizedTextProvider CreateProvider(ApplicationLanguage language) => language.Tag switch
    {
        ApplicationLanguage.EnglishTag => new EnglishTextProvider(),
        ApplicationLanguage.PolishTag => new PolishTextProvider(),
        _ => throw new ArgumentOutOfRangeException(nameof(language))
    };
}
