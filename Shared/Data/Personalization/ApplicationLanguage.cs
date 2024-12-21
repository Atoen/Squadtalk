using System.Globalization;

namespace Shared.Data.Personalization;

public abstract record ApplicationLanguage(string Tag)
{
    private const string EnglishTag = "en";
    private const string PolishTag = "pl";

    public static readonly ApplicationLanguage EnglishLanguage = new English();
    public static readonly ApplicationLanguage PolishLanguage = new Polish();

    public static readonly ApplicationLanguage DefaultLanguage = EnglishLanguage;

    public string Tag { get; } = Tag;
    public CultureInfo CultureInfo { get; } = CultureInfo.GetCultureInfoByIetfLanguageTag(Tag);

    public static ApplicationLanguage ParseLanguageCode(ReadOnlySpan<char> languageCode) => languageCode switch
    {
        EnglishTag => EnglishLanguage,
        PolishTag => PolishLanguage,
        _ => DefaultLanguage
    };

    public sealed record English() : ApplicationLanguage(EnglishTag);
    public sealed record Polish() : ApplicationLanguage(PolishTag);
}
