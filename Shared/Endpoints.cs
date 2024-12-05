namespace Shared;

public static class Endpoints
{
    private const string ApiRoute = "api/";
    private const string ProfileController = ApiRoute + "profile/";

    public const string Login = ProfileController + "login";
    public const string Register = ProfileController + "register";
    public const string LogOut = ProfileController + "logout";
    public const string LogOutExternal = ProfileController + "logout-external";
    public const string ForgotPassword = ProfileController + "forgot-password";
    public const string ResetPassword = ProfileController + "reset-password";
    public const string ChangeUsername = ProfileController + "change-username";
    public const string ChangeEmail = ProfileController + "change-email";
    public const string ChangePassword = ProfileController + "change-password";
}
