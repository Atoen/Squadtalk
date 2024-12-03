namespace Shared;

public static class Endpoints
{
    private const string ApiRoute = "api/";
    private const string ProfileController = "profile/";

    public const string Login = ApiRoute + "profile/login";
    public const string Register = ApiRoute + "profile/register";
    public const string LogOut = ApiRoute + "profile/logout";
    public const string LogOutExternal = ApiRoute + "profile/logout-external";
    public const string ForgotPassword = ApiRoute + "profile/forgot-password";
    public const string ResetPassword = ApiRoute + "profile/reset-password";
}
