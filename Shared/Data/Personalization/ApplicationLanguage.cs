namespace Shared.Data.Personalization;

public sealed record ApplicationLanguage
{
    public const string EnglishTag = "en";
    public const string PolishTag = "pl";

    public static readonly ApplicationLanguage English = new(EnglishTag);
    public static readonly ApplicationLanguage Polish = new(PolishTag);

    public static readonly ApplicationLanguage Default = English;

    public string Tag { get; }

    private ApplicationLanguage(string tag) => Tag = tag;

    public static ApplicationLanguage ParseLanguageCode(ReadOnlySpan<char> languageCode) => languageCode switch
    {
        EnglishTag => English,
        PolishTag => Polish,
        _ => Default
    };
}
