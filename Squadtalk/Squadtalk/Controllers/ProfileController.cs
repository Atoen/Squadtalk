using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Shared.DTOs.Account;
using Squadtalk.Data.Entities;

namespace Squadtalk.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IEmailSender<ApplicationUser> _emailSender;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        IEmailSender<ApplicationUser> emailSender,
        ILogger<ProfileController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _userStore = userStore;
        _emailSender = emailSender;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser(UserLoginDto loginDto, [FromQuery] string? returnUrl)
    {
        var result = await _signInManager.PasswordSignInAsync(loginDto.Username, loginDto.Password, loginDto.Remember, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return Unauthorized("Invalid login attempt");
        }

        if (string.IsNullOrEmpty(returnUrl))
        {
            return Ok();
        }

        return LocalRedirect(returnUrl);
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser(UserRegisterDto registerDto)
    {
        var alreadyRegistered = await _userManager.FindByEmailAsync(registerDto.Email);
        if (alreadyRegistered is not null)
        {
            return Conflict("Email in use");
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
            return BadRequest(result.Errors);
        }

        var userId = await _userManager.GetUserIdAsync(user);
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = Url.Action(
            action: "ConfirmEmail",
            controller: "Profile",
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

        return Ok();
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string? userId, [FromQuery] string? code)
    {
        if (userId is null || code is null)
        {
            return LocalRedirect("/Profile/InvalidEmailConfirmationLink");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        if (user.EmailConfirmed)
        {
            return LocalRedirect("/Profile/AlreadyConfirmed");
        }

        var decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await _userManager.ConfirmEmailAsync(user, decoded);
        var uri = result.Succeeded ? "/Profile/EmailConfirmed" : "/Profile/ConfirmationError";

        return LocalRedirect(uri);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
    {
        var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
        if (user is null || !user.EmailConfirmed)
        {
            return Ok();
        }

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = $"{Request.Scheme}://{Request.Host}/ResetPassword?userId={user.Id}&code={code}";
        await _emailSender.SendPasswordResetLinkAsync(user, forgotPasswordDto.Email, callbackUrl);

        return Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
    {
        var user = await _userManager.FindByIdAsync(resetPasswordDto.UserId);
        if (user is null)
        {
            return Ok();
        }

        var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetPasswordDto.Code));
        var result = await _userManager.ResetPasswordAsync(user, code, resetPasswordDto.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest();
        }

        return Ok();
    }

    [Authorize]
    [HttpPost("change-username")]
    public async Task<IActionResult> ChangeUsername(ChangeUsernameDto changeUsernameDto)
    {
        var user = await _userManager.FindByIdAsync(changeUsernameDto.UserId);
        if (user is null)
        {
            return NotFound();
        }

        if (user.UserName == changeUsernameDto.NewUsername)
        {
            return NoContent();
        }

        var existingUsername = await _userManager.FindByNameAsync(changeUsernameDto.NewUsername);
        if (existingUsername is not null)
        {
            return Conflict();
        }

        var passwordMatches = await _userManager.CheckPasswordAsync(user, changeUsernameDto.Password);
        if (!passwordMatches)
        {
            return Unauthorized();
        }

        var result =  await _userManager.SetUserNameAsync(user, changeUsernameDto.NewUsername);
        if (!result.Succeeded)
        {
            return Problem();
        }

        await _signInManager.RefreshSignInAsync(user);

        var response = new UsernameChangeResultDto
        {
            ChangedUsername = changeUsernameDto.NewUsername
        };

        return Ok(response);
    }

    [Authorize]
    [HttpPost("change-email")]
    public async Task<IActionResult> ChangeEmail(ChangeEmailDto changeEmailDto)
    {
        var user = await _userManager.FindByIdAsync(changeEmailDto.UserId);
        if (user is null)
        {
            return NotFound();
        }

        if (user.Email == changeEmailDto.NewEmail)
        {
            return NoContent();
        }

        var existingEmail = await _userManager.FindByEmailAsync(changeEmailDto.NewEmail);
        if (existingEmail is not null)
        {
            return Conflict();
        }

        var passwordMatches = await _userManager.CheckPasswordAsync(user, changeEmailDto.Password);
        if (!passwordMatches)
        {
            return Unauthorized();
        }

        var code = await _userManager.GenerateChangeEmailTokenAsync(user, changeEmailDto.NewEmail);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = Url.Action(
            action: "ConfirmEmailChange",
            controller: "Profile",
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

        return Ok();
    }

    [HttpGet("confirm-email-change")]
    public async Task<IActionResult> ConfirmEmailChange([FromQuery] string? userId, [FromQuery] string? email, [FromQuery] string? code)
    {
        if (userId is null || email is null || code is null)
        {
            return LocalRedirect("/Profile/InvalidEmailConfirmationLink");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        var decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await _userManager.ChangeEmailAsync(user, email, decoded);
        if (!result.Succeeded)
        {
            return LocalRedirect("/Profile/ConfirmationError");
        }

        await _signInManager.RefreshSignInAsync(user);

        return LocalRedirect("/Profile/EmailConfirmed");
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
    {
        var user = await _userManager.FindByIdAsync(changePasswordDto.UserId);
        if (user is null)
        {
            return NotFound();
        }

        var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
        if (!result.Succeeded)
        {
            return Unauthorized();
        }

        await _signInManager.RefreshSignInAsync(user);

        return Ok();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromQuery] string? returnUrl)
    {
        await _signInManager.SignOutAsync();
        return LocalRedirect($"~/{returnUrl}");
    }

    [HttpGet("logout-external")]
    public async Task<IActionResult> LogoutExternal()
    {
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        return LocalRedirect("/");
    }
}
