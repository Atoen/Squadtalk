namespace Squadtalk.Client.Localization.Providers;

using Squadtalk.Client.Localization;

public class EnglishTextProvider() : LocalizedTextProvider(Dictionary)
{
    private static Dictionary<string, string> Dictionary => new()
    {
        // Navigation
        [nameof(TextTable.Messages)] = "Messages",
        [nameof(TextTable.Home)] = "Home",
        [nameof(TextTable.Canvas)] = "Canvas",
        [nameof(TextTable.Register)] = "Register",
        [nameof(TextTable.Login)] = "Log in",
        [nameof(TextTable.Logout)] = "Log out",
        [nameof(TextTable.Users)] = "Users",
        [nameof(TextTable.Settings)] = "Settings",

        // Home page
        [nameof(TextTable.HelloWorld)] = "Hello world!",
        [nameof(TextTable.WelcomeToApp)] = "Welcome to your new app.",

        // Messages page
        [nameof(TextTable.CreateChannelTooltip)] = "Create new group",
        [nameof(TextTable.CreateChannelModalTitle)] = "New group",
        [nameof(TextTable.CreateGroupChatButton)] = "Create group",
        [nameof(TextTable.SelectUserPlaceholder)] = "Select user",
        [nameof(TextTable.LoadingUsers)] = "Loading...",
        [nameof(TextTable.NoUsersFound)] = "No users found",

        // Settings page
        [nameof(TextTable.SelectLanguage)] = "Select app language",

        // User status
        [nameof(TextTable.Online)] = "Online",
        [nameof(TextTable.Offline)] = "Offline",
        [nameof(TextTable.Away)] = "Away",
        [nameof(TextTable.DoNotDisturb)] = "Do not disturb",

        // Dialog
        [nameof(TextTable.DialogCancel)] = "Cancel",
        [nameof(TextTable.DialogNo)] = "No",
        [nameof(TextTable.DialogYes)] = "Yes",

        // Chat page
        [nameof(TextTable.StartCall)] = "Start call",
        [nameof(TextTable.ManageChannel)] = "Manage group",
        [nameof(TextTable.ChannelBeginning)] = "This is the beginning of chat with ",

        // Chat message
        [nameof(TextTable.TodayTimeTemplate)] = "Today {0}",
        [nameof(TextTable.YesterdayTimeTemplate)] = "Yesterday {0}",
        [nameof(TextTable.ChannelCreatedTemplate)] = "{0} has created this group.",
        [nameof(TextTable.ChannelNameChangedTemplate)] = "{0} has changed this group's name {1}.",
        [nameof(TextTable.CallStartedTemplate)] = "{0} has started voice call.",
        [nameof(TextTable.CallEndedTemplate)] = "{0} has started voice call that lasted {1}.",

        // Call dialog
        [nameof(TextTable.CallIncomingTemplate)] = "Incoming call from {0}",
        [nameof(TextTable.AcceptCallDialogMessage)] = "Accept voice call?",
        [nameof(TextTable.AcceptCall)] = "Accept",
        [nameof(TextTable.DeclineCall)] = "Decline",

        // Hide channel modal
        [nameof(TextTable.HideChannelModalTitle)] = "Hide chat",
        [nameof(TextTable.HideChannelModalBodyTemplate)] = "Hiding this text channel will make it inaccessible unless you send or receive messages from {0}.",
        [nameof(TextTable.HideChannelModalProceed)] = "Are you sure you want to proceed?",

        // Channel last message
        [nameof(TextTable.MessageSentInfo)] = "sent",
        [nameof(TextTable.MessageYouSentInfo)] = "sent",
        [nameof(TextTable.You)] = "You",
        [nameof(TextTable.File)] = "file",
        [nameof(TextTable.Image)] = "image",
        [nameof(TextTable.Video)] = "video",
        [nameof(TextTable.UserCalledTemplate)] = "{0} started a voice call",
        [nameof(TextTable.UserCreatedChannelTemplate)] = "{0} created this group",
        [nameof(TextTable.UserChangedChannelNameTemplate)] = "{0} changed group name to {1}",
        [nameof(TextTable.YouCalled)] = "You started a voice call",
        [nameof(TextTable.YouCreatedChannel)] = "You created this group",
        [nameof(TextTable.YouChangedChannelNameTemplate)] = "You changed name of the group"
    };
}
