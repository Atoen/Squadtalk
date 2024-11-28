using Shared.Data.Personalization;
using TextLocalizer.Types;

namespace Shared.Services;

public interface ITextProviderManager
{
    ILocalizedTextProvider GetProvider(ApplicationLanguage language);
}
