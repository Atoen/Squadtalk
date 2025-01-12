namespace Shared.Routing;

public static class Routes
{
    public static class Pages
    {
        public const string Root = "/";
        public const string Canvas = "/Canvas";
        public const string Simulation = "/Canvas/Simulation";
        public const string Chat = "/Chats/{ChannelId}";
        public const string Chats = "/Chats";
        public const string Contacts = "/Contacts";
        public const string Settings = "/Settings";
        public const string NotFound = "/NotFound";

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
        public const string ApiBase = "/api";
        public const string AccountController = ApiBase + "/account/";

        public const string Login = AccountController + RelativeEndpoints.Login;
        public const string Register = AccountController + RelativeEndpoints.Register;
        public const string LogOut = AccountController + RelativeEndpoints.LogOut;
        public const string LogOutExternal = AccountController + RelativeEndpoints.LogOutExternal;
        public const string ForgotPassword = AccountController + RelativeEndpoints.ForgotPassword;
        public const string ResetPassword = AccountController + RelativeEndpoints.ResetPassword;
        public const string ChangeUsername = AccountController + RelativeEndpoints.ChangeUsername;
        public const string ConfirmEmail = AccountController + RelativeEndpoints.ConfirmEmail;
        public const string ChangeEmail = AccountController + RelativeEndpoints.ChangeEmail;
        public const string ChangePassword = AccountController + RelativeEndpoints.ChangePassword;
        public const string ConfirmEmailChange = AccountController + RelativeEndpoints.ConfirmEmailChange;
    }
}