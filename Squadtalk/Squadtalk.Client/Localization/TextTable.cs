using System.Collections.Concurrent;
using System.Globalization;
using Shared.Enums;
using Squadtalk.Client.Localization.Providers;

namespace Squadtalk.Client.Localization;

public class TextTable
{
    private readonly ConcurrentDictionary<SupportedLanguage, LocalizedTextProvider> _providers = new();

    public SupportedLanguage Language { get; private set; } = DefaultLanguage;

    public CultureInfo CultureInfo { get; private set; }

    public void SetLanguage(SupportedLanguage language)
    {
        Language = language;
        CultureInfo = CultureInfo.GetCultureInfoByIetfLanguageTag(GetLanguageCode(language));
    }

    public static SupportedLanguage ParseLanguageCode(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return DefaultLanguage;
        }

        var normalizedCode = languageCode.ToLowerInvariant().Split('-')[0];
        return normalizedCode switch
        {
            "en" => SupportedLanguage.English,
            "pl" => SupportedLanguage.Polish,
            "de" => SupportedLanguage.German,
            _ => DefaultLanguage
        };
    }

    public static string GetLanguageCode(SupportedLanguage language) => language switch
    {
        SupportedLanguage.English => "en",
        SupportedLanguage.Polish => "pl",
        SupportedLanguage.German => "de",
        _ => string.Empty
    };

    public static SupportedLanguage DefaultLanguage { get; set; } = SupportedLanguage.English;

    private static LocalizedTextProvider CreateProvider(SupportedLanguage language) => language switch
    {
        SupportedLanguage.English => new EnglishTextProvider(),
        SupportedLanguage.Polish => new PolishTextProvider(),
        SupportedLanguage.German => new GermanTextProvider(),
        _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
    };

    private LocalizedTextProvider Provider => _providers.GetOrAdd(Language, CreateProvider);

    // Home page
    public string HelloWorld => Provider[nameof(HelloWorld)];
    public string WelcomeToApp => Provider[nameof(WelcomeToApp)];

    // Navigation
    public string Messages => Provider[nameof(Messages)];
    public string Home => Provider[nameof(Home)];
    public string Canvas => Provider[nameof(Canvas)];
    public string Register => Provider[nameof(Register)];
    public string Login => Provider[nameof(Login)];
    public string Logout => Provider[nameof(Logout)];
    public string Users => Provider[nameof(Users)];
    public string Settings => Provider[nameof(Settings)];
    public string SelectLanguage => Provider[nameof(SelectLanguage)];

    // Dialog
    public string DialogYes => Provider[nameof(DialogYes)];
    public string DialogNo => Provider[nameof(DialogNo)];
    public string DialogCancel => Provider[nameof(DialogCancel)];

    // User status
    public string Online => Provider[nameof(Online)];
    public string Offline => Provider[nameof(Offline)];
    public string Away => Provider[nameof(Away)];
    public string DoNotDisturb => Provider[nameof(DoNotDisturb)];

    // Channel selector
    public string CreateChannelTooltip => Provider[nameof(CreateChannelTooltip)];
    public string CreateChannelModalTitle => Provider[nameof(CreateChannelModalTitle)];
    public string CreateGroupChatButton => Provider[nameof(CreateGroupChatButton)];
    public string SelectUserPlaceholder => Provider[nameof(SelectUserPlaceholder)];
    public string LoadingUsers => Provider[nameof(LoadingUsers)];
    public string NoUsersFound => Provider[nameof(NoUsersFound)];

    // Chat
    public string StartCall => Provider[nameof(StartCall)];
    public string ManageChannel => Provider[nameof(ManageChannel)];
    public string ChannelBeginning => Provider[nameof(ChannelBeginning)];

    // System message
    public string ChannelCreatedTemplate => Provider[nameof(ChannelCreatedTemplate)];
    public string ChannelNameChangedTemplate => Provider[nameof(ChannelNameChangedTemplate)];
    public string CallStartedTemplate => Provider[nameof(CallStartedTemplate)];
    public string CallEndedTemplate => Provider[nameof(CallEndedTemplate)];

    // Call dialog
    public string AcceptCallDialogMessage => Provider[nameof(AcceptCallDialogMessage)];
    public string AcceptCall => Provider[nameof(AcceptCall)];
    public string DeclineCall => Provider[nameof(DeclineCall)];
    public string CallIncomingTemplate => Provider[nameof(CallIncomingTemplate)];

    // User message
    public string TodayTimeTemplate => Provider[nameof(TodayTimeTemplate)];
    public string YesterdayTimeTemplate => Provider[nameof(YesterdayTimeTemplate)];
    
    // Hide channel modal
    public string HideChannelModalTitle => Provider[nameof(HideChannelModalTitle)];
    public string HideChannelModalBodyTemplate => Provider[nameof(HideChannelModalBodyTemplate)];
    public string HideChannelModalProceed => Provider[nameof(HideChannelModalProceed)];

    // Channel last message
    public string File => Provider[nameof(File)];
    public string Image => Provider[nameof(Image)];
    public string Video => Provider[nameof(Video)];

    public string MessageSentInfo => Provider[nameof(MessageSentInfo)];
    public string MessageYouSentInfo => Provider[nameof(MessageYouSentInfo)];

    public string You => Provider[nameof(You)];

    public string UserCalledTemplate => Provider[nameof(UserCalledTemplate)];
    public string UserCreatedChannelTemplate => Provider[nameof(UserCreatedChannelTemplate)];
    public string UserChangedChannelNameTemplate => Provider[nameof(UserChangedChannelNameTemplate)];

    public string YouCalled => Provider[nameof(YouCalled)];
    public string YouCreatedChannel => Provider[nameof(YouCreatedChannel)];
    public string YouChangedChannelNameTemplate => Provider[nameof(YouChangedChannelNameTemplate)];

}
