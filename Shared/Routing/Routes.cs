namespace Shared.Routing;

public static class Routes
{
    public static class Pages
    {
        public const string Root = "/";
        public const string Canvas = "/Canvas";
        public const string Simulation = "/Canvas/Simulation";
        public const string Chat = "/Channels/{ChannelId}";
        public const string Chats = "/Chats";
        public const string Users = "/Users";
        public const string Settings = "/Settings";

        public const string ForgotPassword = "/ForgotPassword";
        public const string ResetPassword = "/ResetPassword";

        public const string Profile = "/Profile";
        public const string Login = Profile + "/Login";
        public const string Register = Profile + "/Register";

        public const string ConfirmEmail = Profile + "/ConfirmEmail";
        public const string EmailConfirmed = Profile + "/EmailConfirmed";
        public const string EmailAlreadyConfirmed = Profile + "/AlreadyConfirmed";
        public const string EmailConfirmationError = Profile + "/ConfrimationError";
        public const string InvalidEmailConfirmationLink = Profile + "/InvalidEmailConfirmationLink";
        public const string InvalidPasswordReset = Profile + "/InvalidPasswordReset";
        public const string ForgotPasswordConfirmation = Profile + "/ForgotPasswordConfirmation";
        public const string ResetPasswordConfirmation = Profile + "/ResetPasswordConfirmation";
    }

    public static class RelativeEndpoints
    {
        public const string Login = "login";
        public const string Register = "register";
        public const string LogOut = "logout";
        public const string LogOutExternal = "logout-external";
        public const string ForgotPassword = "forgot-password";
        public const string ResetPassword = "reset-password";
        public const string ChangeUsername = "change-username";
        public const string ConfirmEmail = "confirm-email";
        public const string ChangeEmail = "change-email";
        public const string ChangePassword = "change-password";
        public const string ConfirmEmailChange = "confirm-email-change";
    }

    public static class Endpoints
    {
        private const string ApiRoute = "/api/";
        public const string ProfileController = ApiRoute + "profile/";

        public const string Login = ProfileController + RelativeEndpoints.Login;
        public const string Register = ProfileController + RelativeEndpoints.Register;
        public const string LogOut = ProfileController + RelativeEndpoints.LogOut;
        public const string LogOutExternal = ProfileController + RelativeEndpoints.LogOutExternal;
        public const string ForgotPassword = ProfileController + RelativeEndpoints.ForgotPassword;
        public const string ResetPassword = ProfileController + RelativeEndpoints.ResetPassword;
        public const string ChangeUsername = ProfileController + RelativeEndpoints.ChangeUsername;
        public const string ConfirmEmail = ProfileController + RelativeEndpoints.ConfirmEmail;
        public const string ChangeEmail = ProfileController + RelativeEndpoints.ChangeEmail;
        public const string ChangePassword = ProfileController + RelativeEndpoints.ChangePassword;
        public const string ConfirmEmailChange = ProfileController + RelativeEndpoints.ConfirmEmailChange;
    }
}
