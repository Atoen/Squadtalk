using TextLocalizer;

namespace Squadtalk.Client.Localization;

[TranslationProvider(Filename = "english.yml", IsDefault = true)]
public partial class EnglishTextProvider;

[TranslationProvider(Filename = "polish.yml")]
public partial class PolishTextProvider;
