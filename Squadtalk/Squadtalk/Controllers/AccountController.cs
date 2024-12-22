using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Shared.DTOs.Account;
using Shared.DTOs.Account.Results;
using Shared.Routing;
using Shared.Services;
using Squadtalk.Data.Entities;

namespace Squadtalk.Controllers;

[ApiController]
[Route(Routes.Endpoints.AccountController)]
public class AccountController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IEmailSender<ApplicationUser> _emailSender;
    private readonly IAccountManager _accountManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        IEmailSender<ApplicationUser> emailSender,
        IAccountManager accountManager,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _userStore = userStore;
        _emailSender = emailSender;
        _accountManager = accountManager;
        _logger = logger;
    }

    [HttpPost(Routes.RelativeEndpoints.Login)]
    public async Task<ActionResult<LoginResultDto>> LoginUser(UserLoginDto loginDto)
    {
        var result = await _signInManager.PasswordSignInAsync(loginDto.Username, loginDto.Password, loginDto.Remember, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return Unauthorized(LoginResultDto.Fail);
        }

        return Ok(LoginResultDto.Success);
    }

    [HttpPost(Routes.RelativeEndpoints.Register)]
    public async Task<ActionResult<ResetPasswordResultDto>> RegisterUser(UserRegisterDto registerDto)
    {
        var alreadyUsingEmail = await _userManager.FindByEmailAsync(registerDto.Email);
        if (alreadyUsingEmail is not null)
        {
            return Conflict(RegisterResultDto.EmailInUse);
        }

        var alreadyUsingUsername = await _userManager.FindByNameAsync(registerDto.Username);
        if (alreadyUsingUsername is not null)
        {
            return Conflict(RegisterResultDto.UsernameInUse);
        }

        var user = new ApplicationUser();
        await _userStore.SetUserNameAsync(user, registerDto.Username, HttpContext.RequestAborted);

        if (_userStore is IUserEmailStore<ApplicationUser> emailStore)
        {
            await emailStore.SetEmailAsync(user, registerDto.Email, HttpContext.RequestAborted);
        }

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(RegisterResultDto.FailedToCreateAccount);
        }

        var userId = await _userManager.GetUserIdAsync(user);
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = Url.Action(
            action: "ConfirmEmail",
            controller: "Account",
            values: new { userId, code },
            protocol: Request.Scheme);

        if (callbackUrl is null)
        {
            _logger.LogError("Callback url is null");
        }
        else
        {
            await _emailSender.SendConfirmationLinkAsync(user, registerDto.Email, callbackUrl);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);

        return Ok(RegisterResultDto.Success);
    }

    [HttpGet(Routes.RelativeEndpoints.ConfirmEmail)]
    public async Task<ActionResult> ConfirmEmail([FromQuery] string? userId, [FromQuery] string? code)
    {
        if (userId is null || code is null)
        {
            return LocalRedirect(Routes.Pages.InvalidEmailConfirmationLink);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return LocalRedirect(Routes.Pages.InvalidEmailConfirmationLink);
        }

        if (user.EmailConfirmed)
        {
            return LocalRedirect(Routes.Pages.EmailAlreadyConfirmed);
        }

        var decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await _userManager.ConfirmEmailAsync(user, decoded);
        if (!result.Succeeded)
        {
            var errorMessages = string.Join("; ", result.Errors.Select(e => e.Description));
            _logger.LogError("Error while confirming email: {ErrorMessages}", errorMessages);
            return LocalRedirect(Routes.Pages.EmailConfirmationError);
        }

        return LocalRedirect(Routes.Pages.EmailConfirmed);
    }

    [HttpPost(Routes.RelativeEndpoints.ForgotPassword)]
    public async Task<ActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
    {
        var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
        if (user is null || !user.EmailConfirmed)
        {
            return Ok();
        }

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = $"{Request.Scheme}://{Request.Host}{Routes.Pages.ResetPassword}?userId={user.Id}&code={code}";
        await _emailSender.SendPasswordResetLinkAsync(user, forgotPasswordDto.Email, callbackUrl);

        return Ok();
    }

    [HttpPost(Routes.RelativeEndpoints.ResetPassword)]
    public async Task<ActionResult<ResetPasswordResultDto>> ResetPassword(ResetPasswordDto resetPasswordDto)
    {
        var user = await _userManager.FindByIdAsync(resetPasswordDto.UserId);
        if (user is null)
        {
            return Ok(ResetPasswordResultDto.Success);
        }

        var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetPasswordDto.Code));
        var result = await _userManager.ResetPasswordAsync(user, code, resetPasswordDto.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(ResetPasswordResultDto.Fail);
        }

        return Ok(ResetPasswordResultDto.Success);
    }

    [Authorize]
    [HttpPost(Routes.RelativeEndpoints.ChangeUsername)]
    public async Task<ActionResult<ChangeUsernameResultDto>> ChangeUsername(ChangeUsernameDto changeUsernameDto)
    {
        var user = await _userManager.FindByIdAsync(changeUsernameDto.UserId);
        if (user is null)
        {
            return NotFound(ChangeUsernameResultDto.NotFound);
        }

        var passwordMatches = await _userManager.CheckPasswordAsync(user, changeUsernameDto.Password);
        if (!passwordMatches)
        {
            return Unauthorized(ChangeUsernameResultDto.Unauthorized);
        }

        if (user.NormalizedUserName == _userManager.NormalizeName(changeUsernameDto.NewUsername))
        {
            return Ok(ChangeUsernameResultDto.NotChanged);
        }

        var existingUsername = await _userManager.FindByNameAsync(changeUsernameDto.NewUsername);
        if (existingUsername is not null)
        {
            return Conflict(ChangeUsernameResultDto.UsernameInUse);
        }

        var result =  await _userManager.SetUserNameAsync(user, changeUsernameDto.NewUsername);
        if (!result.Succeeded)
        {
            return BadRequest(ChangeUsernameResultDto.FailedToChange);
        }

        await _signInManager.RefreshSignInAsync(user);
        return Ok(ChangeUsernameResultDto.Success(changeUsernameDto.NewUsername));
    }

    [Authorize]
    [HttpPost(Routes.RelativeEndpoints.ChangeEmail)]
    public async Task<ActionResult<ChangeEmailResultDto>> ChangeEmail(ChangeEmailDto changeEmailDto)
    {
        var user = await _userManager.FindByIdAsync(changeEmailDto.UserId);
        if (user is null)
        {
            return NotFound(ChangeEmailResultDto.NotFound);
        }

        var passwordMatches = await _userManager.CheckPasswordAsync(user, changeEmailDto.Password);
        if (!passwordMatches)
        {
            return Unauthorized(ChangeEmailResultDto.Unauthorized);
        }

        if (user.NormalizedEmail == _userManager.NormalizeEmail(changeEmailDto.NewEmail))
        {
            return Ok(ChangeEmailResultDto.NotChanged);
        }

        var existingEmail = await _userManager.FindByEmailAsync(changeEmailDto.NewEmail);
        if (existingEmail is not null && existingEmail.Id != user.Id)
        {
            return Conflict(ChangeEmailResultDto.EmailInUse);
        }

        var code = await _userManager.GenerateChangeEmailTokenAsync(user, changeEmailDto.NewEmail);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = Url.Action(
            action: "ConfirmEmailChange",
            controller: "Account",
            values: new { userId = changeEmailDto.UserId, email = changeEmailDto.NewEmail, code },
            protocol: Request.Scheme);

        if (callbackUrl is null)
        {
            _logger.LogError("Callback url is null");
        }
        else
        {
            await _emailSender.SendConfirmationLinkAsync(user, changeEmailDto.NewEmail, callbackUrl);
        }

        return Ok(ChangeEmailResultDto.ConfirmationSent);
    }

    [HttpGet(Routes.RelativeEndpoints.ConfirmEmailChange)]
    public async Task<ActionResult> ConfirmEmailChange([FromQuery] string? userId, [FromQuery] string? email, [FromQuery] string? code)
    {
        if (userId is null || email is null || code is null)
        {
            return LocalRedirect(Routes.Pages.InvalidEmailConfirmationLink);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return LocalRedirect(Routes.Pages.InvalidEmailConfirmationLink);
        }

        var decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await _userManager.ChangeEmailAsync(user, email, decoded);
        if (!result.Succeeded)
        {
            return LocalRedirect(Routes.Pages.EmailConfirmationError);
        }

        await _signInManager.RefreshSignInAsync(user);

        return LocalRedirect(Routes.Pages.EmailConfirmed);
    }

    [Authorize]
    [HttpPost(Routes.RelativeEndpoints.ChangePassword)]
    public async Task<ActionResult<ChangePasswordResultDto>> ChangePassword(ChangePasswordDto changePasswordDto)
    {
        var user = await _userManager.FindByIdAsync(changePasswordDto.UserId);
        if (user is null)
        {
            return NotFound(ChangePasswordResultDto.Fail);
        }

        var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
        if (!result.Succeeded)
        {
            return NotFound(ChangePasswordResultDto.Fail);
        }

        await _signInManager.RefreshSignInAsync(user);

        return NotFound(ChangePasswordResultDto.Success);
    }

    [HttpPost(Routes.RelativeEndpoints.LogOut)]
    public async Task<ActionResult> Logout([FromQuery] string? returnUrl)
    {
        await _signInManager.SignOutAsync();
        return LocalRedirect($"~/{returnUrl}");
    }

    [HttpGet(Routes.RelativeEndpoints.LogOutExternal)]
    public async Task<ActionResult> LogoutExternal()
    {
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        return LocalRedirect(Routes.Pages.Root);
    }
}
