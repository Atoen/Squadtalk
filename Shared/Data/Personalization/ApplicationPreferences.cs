namespace Shared.Data.Personalization;

public readonly ref struct ApplicationPreferences
{
    private const char Delimiter = '$';
    private const int MaxStackAllocLength = 128;

    public readonly ApplicationLanguage Language = ApplicationLanguage.DefaultLanguage;
    public readonly ApplicationTheme Theme = ApplicationTheme.DefaultTheme;
    public readonly ApplicationPalette Palette = ApplicationPalette.DefaultPalette;
    public readonly ApplicationTheme.Automatic.Mode AutoMode = default;

    public ApplicationPreferences(ApplicationLanguage language, ApplicationTheme theme, ApplicationPalette palette, ApplicationTheme.Automatic.Mode autoMode)
    {
        Language = language;
        Theme = theme;
        Palette = palette;
        AutoMode = autoMode;
    }

    public static ApplicationPreferences Default => new();

    public static ApplicationPreferences Parse(ReadOnlySpan<char> data)
    {
        if (data.IsEmpty)
        {
            return Default;
        }

        var splitEnumerator = data.Split(Delimiter).GetEnumerator();

        splitEnumerator.MoveNext();
        var language = ApplicationLanguage.ParseLanguageCode(data[splitEnumerator.Current]);

        splitEnumerator.MoveNext();
        var theme = ApplicationTheme.ParseValue(data[splitEnumerator.Current]);

        splitEnumerator.MoveNext();
        var palette = ApplicationPalette.ParseValue(data[splitEnumerator.Current]);

        ApplicationTheme.Automatic.Mode autoMode = default;
        if (theme is ApplicationTheme.Automatic)
        {
            splitEnumerator.MoveNext();
            autoMode = ApplicationThemeExtensions.ParseAutoModeValue(data[splitEnumerator.Current]);
        }

        return new ApplicationPreferences(language, theme, palette, autoMode);
    }

    public string Serialize()
    {
        var languageLength = Language.Tag.Length;
        var themeLength = Theme.Value.Length;
        var paletteLength = Palette.Value.Length;

        var totalLength = languageLength + themeLength + paletteLength + 2;
        var usesAutoTheme = Theme is ApplicationTheme.Automatic;
        if (usesAutoTheme)
        {
            totalLength += AutoMode.Value().Length + 1;
        }

        var span = totalLength <= MaxStackAllocLength
            ? stackalloc char[totalLength]
            : new char[totalLength];

        var position = 0;
        Language.Tag.CopyTo(span[position..]);
        position += languageLength;

        span[position++] = Delimiter;

        Theme.Value.CopyTo(span[position..]);
        position += themeLength;

        span[position++] = Delimiter;

        Palette.Value.CopyTo(span[position..]);
        position += paletteLength;

        if (usesAutoTheme)
        {
            span[position++] = Delimiter;
            AutoMode.Value().CopyTo(span[position..]);
        }

        return new string(span);
    }
}
