using Refit;
using Shared.DTOs.Account;
using Shared.DTOs.Account.Results;
using Shared.Routing;

namespace Squadtalk.Client.Network;

internal interface IAccountApi
{
    [Post(Routes.Endpoints.Login)]
    Task<LoginResultDto> Login(UserLoginDto loginDto);

    [Post(Routes.Endpoints.Register)]
    Task<RegisterResultDto> Register(UserRegisterDto registerDto);

    [Post(Routes.Endpoints.ForgotPassword)]
    Task<IApiResponse> ForgotPassword(ForgotPasswordDto forgotPasswordDto);

    [Post(Routes.Endpoints.ResetPassword)]
    Task<ResetPasswordResultDto> ResetPassword(ResetPasswordDto resetPasswordDto);

    [Post(Routes.Endpoints.ChangeUsername)]
    Task<ChangeUsernameResultDto> ChangeUsername(ChangeUsernameDto changeUsernameDto);

    [Post(Routes.Endpoints.ChangeEmail)]
    Task<ChangeEmailResultDto> ChangeEmail(ChangeEmailDto changeEmailDto);

    [Post(Routes.Endpoints.ChangePassword)]
    Task<ChangePasswordResultDto> ChangePassword(ChangePasswordDto changePasswordDto);
}
