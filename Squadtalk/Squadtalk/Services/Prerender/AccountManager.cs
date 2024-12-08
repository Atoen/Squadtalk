using Shared.Data.Results;
using Shared.DTOs.Account;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

public class AccountManager : IAccountManager
{
    public Task<LoginResult> LoginAsync(UserLoginDto loginDto) => throw new InvalidOperationException();
    public Task<RegisterResult> RegisterAsync(UserRegisterDto registerDto) => throw new InvalidOperationException();
    public Task<ForgotPasswordResult> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto) => throw new InvalidOperationException();
    public Task<ResetPasswordResult> ResetPasswordAsync(ResetPasswordDto resetPasswordDto) => throw new InvalidOperationException();
    public Task<ChangeUsernameResult> ChangeUsernameAsync(ChangeUsernameDto changeUsernameDto) => throw new InvalidOperationException();
    public Task<ChangeEmailResult> ChangeEmailAsync(ChangeEmailDto changeEmailDto) => throw new InvalidOperationException();
    public Task<ChangePasswordResult> ChangePasswordAsync(ChangePasswordDto changePasswordDto) => throw new InvalidOperationException();
}
