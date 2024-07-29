using System.Collections.Concurrent;
using System.Globalization;
using Shared.Enums;
using Squadtalk.Client.Localization.Providers;

namespace Squadtalk.Client.Localization;

public class TextTable
{
    private readonly ConcurrentDictionary<SupportedLanguage, LocalizedTextProvider> _providers = new();

    public static SupportedLanguage DefaultLanguage { get; set; } = SupportedLanguage.English;

    public SupportedLanguage Language { get; private set; } = DefaultLanguage;

    public CultureInfo CultureInfo { get; private set; } = CultureInfo.GetCultureInfoByIetfLanguageTag(GetLanguageCode(DefaultLanguage));

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

    private static LocalizedTextProvider CreateProvider(SupportedLanguage language) => language switch
    {
        SupportedLanguage.English => new EnglishTextProvider(),
        SupportedLanguage.Polish => new PolishTextProvider(),
        SupportedLanguage.German => new GermanTextProvider(),
        _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
    };

    private LocalizedTextProvider CurrentLanguageProvider => _providers.GetOrAdd(Language, CreateProvider);

    // Navigation
    public string Messages => CurrentLanguageProvider[nameof(Messages)];
    public string Home => CurrentLanguageProvider[nameof(Home)];
    public string Canvas => CurrentLanguageProvider[nameof(Canvas)];
    public string Register => CurrentLanguageProvider[nameof(Register)];
    public string Login => CurrentLanguageProvider[nameof(Login)];
    public string Logout => CurrentLanguageProvider[nameof(Logout)];
    public string Users => CurrentLanguageProvider[nameof(Users)];
    public string Settings => CurrentLanguageProvider[nameof(Settings)];

    // Messages page
    public string CreateChannelTooltip => CurrentLanguageProvider[nameof(CreateChannelTooltip)];
    public string CreateChannelModalTitle => CurrentLanguageProvider[nameof(CreateChannelModalTitle)];
    public string CreateGroupChatButton => CurrentLanguageProvider[nameof(CreateGroupChatButton)];
    public string SelectUserPlaceholder => CurrentLanguageProvider[nameof(SelectUserPlaceholder)];
    public string LoadingUsers => CurrentLanguageProvider[nameof(LoadingUsers)];
    public string NoUsersFound => CurrentLanguageProvider[nameof(NoUsersFound)];

    // Chat page
    public string StartCall => CurrentLanguageProvider[nameof(StartCall)];
    public string ManageChannel => CurrentLanguageProvider[nameof(ManageChannel)];
    public string ChannelBeginning => CurrentLanguageProvider[nameof(ChannelBeginning)];

    // Home page
    public string HelloWorld => CurrentLanguageProvider[nameof(HelloWorld)];
    public string WelcomeToApp => CurrentLanguageProvider[nameof(WelcomeToApp)];

    // Settings page
    public string SelectLanguage => CurrentLanguageProvider[nameof(SelectLanguage)];

    // Canvas page
    public string CanvasRandomFill => CurrentLanguageProvider[nameof(CanvasRandomFill)];
    public string CanvasSimulation => CurrentLanguageProvider[nameof(CanvasSimulation)];
    public string CanvasStart => CurrentLanguageProvider[nameof(CanvasStart)];
    public string CanvasStop => CurrentLanguageProvider[nameof(CanvasStop)];
    public string CanvasApply => CurrentLanguageProvider[nameof(CanvasApply)];

    // Simulation page
    public string SimulationRules => CurrentLanguageProvider[nameof(SimulationRules)];
    public string SimulationAddRule => CurrentLanguageProvider[nameof(SimulationAddRule)];
    public string SimulationShareSettings => CurrentLanguageProvider[nameof(SimulationShareSettings)];
    public string SimulationShareSettingsFailed => CurrentLanguageProvider[nameof(SimulationShareSettingsFailed)];
    public string SimulationStep => CurrentLanguageProvider[nameof(SimulationStep)];
    public string SimulationThreshold => CurrentLanguageProvider[nameof(SimulationThreshold)];
    public string SimulationRulesCopied => CurrentLanguageProvider[nameof(SimulationRulesCopied)];
    public string SimulationModalTitle => CurrentLanguageProvider[nameof(SimulationModalTitle)];
    public string SimulationAttacker => CurrentLanguageProvider[nameof(SimulationAttacker)];
    public string SimulationAttacked => CurrentLanguageProvider[nameof(SimulationAttacked)];
    public string SimulationSelectAttacker => CurrentLanguageProvider[nameof(SimulationSelectAttacker)];
    public string SimulationSelectAttacked => CurrentLanguageProvider[nameof(SimulationSelectAttacked)];

    // Dialog
    public string DialogYes => CurrentLanguageProvider[nameof(DialogYes)];
    public string DialogNo => CurrentLanguageProvider[nameof(DialogNo)];
    public string DialogCancel => CurrentLanguageProvider[nameof(DialogCancel)];

    // User status
    public string Online => CurrentLanguageProvider[nameof(Online)];
    public string Offline => CurrentLanguageProvider[nameof(Offline)];
    public string Away => CurrentLanguageProvider[nameof(Away)];
    public string DoNotDisturb => CurrentLanguageProvider[nameof(DoNotDisturb)];

    // Chat message
    public StringTemplate TodayTimeTemplate => CurrentLanguageProvider[nameof(TodayTimeTemplate)];
    public StringTemplate YesterdayTimeTemplate => CurrentLanguageProvider[nameof(YesterdayTimeTemplate)];
    public StringTemplate ChannelCreatedTemplate => CurrentLanguageProvider[nameof(ChannelCreatedTemplate)];
    public StringTemplate ChannelNameChangedTemplate => CurrentLanguageProvider[nameof(ChannelNameChangedTemplate)];
    public StringTemplate ChannelNameClearedTemplate => CurrentLanguageProvider[nameof(ChannelNameClearedTemplate)];
    public StringTemplate CallStartedTemplate => CurrentLanguageProvider[nameof(CallStartedTemplate)];
    public StringTemplate CallEndedTemplate => CurrentLanguageProvider[nameof(CallEndedTemplate)];

    // Call dialog
    public string AcceptCallDialogMessage => CurrentLanguageProvider[nameof(AcceptCallDialogMessage)];
    public string AcceptCall => CurrentLanguageProvider[nameof(AcceptCall)];
    public string DeclineCall => CurrentLanguageProvider[nameof(DeclineCall)];
    public StringTemplate CallIncomingTemplate => CurrentLanguageProvider[nameof(CallIncomingTemplate)];

    // Hide channel modal
    public string HideChannelModalTitle => CurrentLanguageProvider[nameof(HideChannelModalTitle)];
    public StringTemplate HideChannelModalBodyTemplate => CurrentLanguageProvider[nameof(HideChannelModalBodyTemplate)];
    public string HideChannelModalProceed => CurrentLanguageProvider[nameof(HideChannelModalProceed)];

    // Channel last message
    public string You => CurrentLanguageProvider[nameof(You)];
    public string MessageSentInfo => CurrentLanguageProvider[nameof(MessageSentInfo)];
    public string MessageYouSentInfo => CurrentLanguageProvider[nameof(MessageYouSentInfo)];
    public string File => CurrentLanguageProvider[nameof(File)];
    public string Image => CurrentLanguageProvider[nameof(Image)];
    public string Video => CurrentLanguageProvider[nameof(Video)];
    public StringTemplate UserCalledTemplate => CurrentLanguageProvider[nameof(UserCalledTemplate)];
    public StringTemplate UserCreatedChannelTemplate => CurrentLanguageProvider[nameof(UserCreatedChannelTemplate)];
    public StringTemplate UserChangedChannelNameTemplate => CurrentLanguageProvider[nameof(UserChangedChannelNameTemplate)];
    public StringTemplate UserClearedChannelNameTemplate => CurrentLanguageProvider[nameof(UserClearedChannelNameTemplate)];
    public string YouCalled => CurrentLanguageProvider[nameof(YouCalled)];
    public string YouCreatedChannel => CurrentLanguageProvider[nameof(YouCreatedChannel)];
    public StringTemplate YouChangedChannelNameTemplate => CurrentLanguageProvider[nameof(YouChangedChannelNameTemplate)];
    public string YouClearedChannelNameTemplate => CurrentLanguageProvider[nameof(YouClearedChannelNameTemplate)];
}
