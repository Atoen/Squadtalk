using Microsoft.AspNetCore.Identity;
using Shared.Data.Results;
using Shared.DTOs.Account;
using Shared.Services;
using Squadtalk.Data.Entities;

namespace Squadtalk.Services;

internal class AccountManager : IAccountManager
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly ILogger<AccountManager> _logger;

    public AccountManager(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        ILogger<AccountManager> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _userStore = userStore;
        _logger = logger;
    }

    public async Task<LoginResult> LoginAsync(UserLoginDto loginDto)
    {
        var result = await _signInManager.PasswordSignInAsync(loginDto.UsernameOrEmail, loginDto.Password, loginDto.Remember, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return new LoginResult.Fail();
        }

        return new LoginResult.Success();
    }

    public Task<RegisterResult> RegisterAsync(UserRegisterDto registerDto)
    {
        throw new InvalidOperationException();
    }

    public Task<ForgotPasswordResult> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        throw new InvalidOperationException();
    }

    public Task<ResetPasswordResult> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        throw new InvalidOperationException();
    }

    public Task<ChangeUsernameResult> ChangeUsernameAsync(ChangeUsernameDto changeUsernameDto)
    {
        throw new InvalidOperationException();
    }

    public Task<ChangeEmailResult> ChangeEmailAsync(ChangeEmailDto changeEmailDto)
    {
        throw new InvalidOperationException();
    }

    public Task<ChangePasswordResult> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
    {
        throw new InvalidOperationException();
    }
}
