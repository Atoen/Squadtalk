namespace Squadtalk.Client.Localization.Providers;

public class PolishTextProvider() : LocalizedTextProvider(Dictionary)
{
    private static Dictionary<string, string> Dictionary => new()
    {
        [nameof(TextTable.HelloWorld)] = "Witaj świecie!",
        [nameof(TextTable.WelcomeToApp)] = "Witaj w swojej nowej aplikacji.",

        [nameof(TextTable.Messages)] = "Wiadomości",
        [nameof(TextTable.Home)] = "Strona główna",
        [nameof(TextTable.Canvas)] = "Płótno",
        [nameof(TextTable.Register)] = "Rejestracja",
        [nameof(TextTable.Login)] = "Logowanie",
        [nameof(TextTable.Logout)] = "Wyloguj się",
        [nameof(TextTable.Users)] = "Użytkownicy",
        [nameof(TextTable.Settings)] = "Ustawienia",
        [nameof(TextTable.SelectLanguage)] = "Wybierz język aplikacji",

        [nameof(TextTable.Online)] = "Online",
        [nameof(TextTable.Offline)] = "Offline",
        [nameof(TextTable.Away)] = "Zaraz wracam",
        [nameof(TextTable.DoNotDisturb)] = "Zajęty",
        
        [nameof(TextTable.DialogCancel)] = "Anuluj",
        [nameof(TextTable.DialogNo)] = "Nie",
        [nameof(TextTable.DialogYes)] = "Tak",
        
        [nameof(TextTable.CreateChannelTooltip)] = "Utwór nową grupę",
        [nameof(TextTable.CreateChannelModalTitle)] = "Nowa grupa",
        [nameof(TextTable.CreateGroupChatButton)] = "Utwórz grupę",
        [nameof(TextTable.SelectUserPlaceholder)] = "Wybierz użytkownika",
        [nameof(TextTable.LoadingUsers)] = "Ładowanie...",
        [nameof(TextTable.NoUsersFound)] = "Nie znaleziono użytkownika",

        [nameof(TextTable.StartCall)] = "Rozpocznij rozmowę",
        [nameof(TextTable.ManageChannel)] = "Zarządzaj grupą",
        [nameof(TextTable.ChannelBeginning)] = "To jest początek wiadomości z ",
        [nameof(TextTable.ChannelCreatedTemplate)] = "{0} utworzył(a) tę grupę.",
        [nameof(TextTable.ChannelNameChangedTemplate)] = "{0} zmienił(a) nazwę grupy na {1}.",
        [nameof(TextTable.CallStartedTemplate)] = "{0} rozpoczął(ęła)  rozmowę głosową.",
        [nameof(TextTable.CallEndedTemplate)] = "{0} rozpoczął(ęła) rozmowę głosową, która trwała {1}.",

        [nameof(TextTable.CallIncomingTemplate)] = "Połączenie przychodzące od {0}",
        [nameof(TextTable.AcceptCallDialogMessage)] = "Zaakceptować połączenie głosowe?",
        [nameof(TextTable.AcceptCall)] = "Akceptuj",
        [nameof(TextTable.DeclineCall)] = "Odrzuć",

        [nameof(TextTable.TodayTimeTemplate)] = "Dzisiaj o {0}",
        [nameof(TextTable.YesterdayTimeTemplate)] = "Wczoraj o {0}",

        [nameof(TextTable.HideChannelModalTitle)] = "Ukryj rozmowę",
        [nameof(TextTable.HideChannelModalBodyTemplate)] = "Ukrycie tej rozmowy sprawi, że będzie on niedostępny, chyba że wyślesz lub otrzymasz wiadomość od {0}.",
        [nameof(TextTable.HideChannelModalProceed)] = "Czy chcesz kontynuować?",

        [nameof(TextTable.File)] = "plik",
        [nameof(TextTable.Image)] = "obraz",
        [nameof(TextTable.Video)] = "wideo",

        [nameof(TextTable.MessageSentInfo)] = "wysłał(a)",
        [nameof(TextTable.MessageYouSentInfo)] = "wysłano",
        [nameof(TextTable.You)] = "Ty",

        [nameof(TextTable.UserCalledTemplate)] = "{0} rozpoczął rozmowę głosową",
        [nameof(TextTable.UserCreatedChannelTemplate)] = "{0} utworzył grupę",
        [nameof(TextTable.UserChangedChannelNameTemplate)] = "{0} zmienił nazwę grupy na {1}",

        [nameof(TextTable.YouCalled)] = "Rozpocząłeś(aś) rozmowę głosową",
        [nameof(TextTable.YouCreatedChannel)] = "Utworzyłeś(aś) grupę",
        [nameof(TextTable.YouChangedChannelNameTemplate)] = "Zmieniłeś(aś) nazwę grupy"
    };
}
