using Shared.Data.Results;
using Shared.DTOs.Account;

namespace Shared.Services;

public interface IAccountManager
{
    Task<LoginResult> LoginAsync(UserLoginDto loginDto);

    Task<RegisterResult> RegisterAsync(UserRegisterDto registerDto);

    Task<ForgotPasswordResult> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);

    Task<ResetPasswordResult> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);

    Task<ChangeUsernameResult> ChangeUsernameAsync(ChangeUsernameDto changeUsernameDto);

    Task<ChangeEmailResult> ChangeEmailAsync(ChangeEmailDto changeEmailDto);

    Task<ChangePasswordResult> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
}
