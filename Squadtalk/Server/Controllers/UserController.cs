using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Squadtalk.Server.Models;
using Squadtalk.Server.Services;
using Squadtalk.Shared;

namespace Squadtalk.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[SuppressMessage("ReSharper", "UnusedParameter.Local")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ITokenService _tokenService;
    private const string CookieName = "refreshToken";
    
    public UserController(UserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser(UserCredentialsDto userCredentialsDto)
    {
        var result = await _userService.LoginAsync(userCredentialsDto);
        var response = result.Match<IActionResult>(
            success => AuthSuccess(success.Value),
            notFound => NotFound(),
            unauthorized => Unauthorized());

        return response;
    }
    
    [HttpPost("signup")]
    public async Task<IActionResult> RegisterUser(UserCredentialsDto userCredentialsDto)
    {
        var result = await _userService.RegisterAsync(userCredentialsDto);
        var response = result.Match<IActionResult>(
            success => AuthSuccess(success.Value),
            conflict => Conflict(),
            internalError => Problem(internalError.Value));

        return response;
    }
    
    private IActionResult AuthSuccess(ValueTuple<User, RefreshToken> tuple)
    {
        var (user, token) = tuple;
        SetUserRefreshTokenCookie(user, token);
        
        return Ok();
    }
    
    [HttpPost("token")]
    public async Task<IActionResult> LoginUsingToken()
    {
        var cookie = Request.Cookies[CookieName];
        var result = await _userService.VerifyRefreshTokenAsync(cookie);

        var response = result.Match<IActionResult>(
            success => Ok(CreateJwt(success.Value)),
            notFound => NotFound(),
            unauthorized => Unauthorized());
        
        return response;
    }

    [Authorize]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshJwt() => await LoginUsingToken();

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> LogOut(bool invalidateAllSessions = false)
    {
        var userResult = await _userService.GetUserAsync(HttpContext.User, true);
        if (userResult.IsT1) return NotFound();

        var user = userResult.AsT0;

        var cookieFormatResult = _userService.GetFormattedCookieContent(Request.Cookies[CookieName]);
        if (cookieFormatResult.IsT1) return BadRequest();

        var cookie = cookieFormatResult.AsT0;
        var token = cookie.refreshToken;
        
        Response.Cookies.Delete(CookieName);
        if (invalidateAllSessions)
        {
            await _tokenService.RevokeAllRefreshTokens(user);
            return Ok();
        }
        
        var removed = await _tokenService.RevokeRefreshToken(user, token);
        
        return removed ? Ok() : BadRequest();
    }
    
    private string CreateJwt(User user)
    {
        return _tokenService.CreateAuthToken(
            new Claim(JwtClaims.Username, user.Username),
            new Claim(JwtClaims.Role, "user"),
            new Claim(JwtClaims.Uid, user.Id.ToString()));
    }

    private void SetUserRefreshTokenCookie(User user, RefreshToken token)
    {
        var cookie = new CookieOptions
        {
            HttpOnly = true,
            Expires = token.Expires,
            IsEssential = true,
            SameSite = SameSiteMode.Lax
        };

        var value = $"{user.Username} {token.Token}";
        Response.Cookies.Append(CookieName, value, cookie);
    }
}