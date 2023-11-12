using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Squadtalk.Client.Services;

namespace Squadtalk.Client;

public sealed class JwtAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly JwtService _jwtService;
    private AuthenticationState? _authenticationState;

    public JwtAuthenticationStateProvider(JwtService jwtService)
    {
        _jwtService = jwtService;
        _jwtService.TokenUpdated += JwtServiceOnTokenUpdated;
    }

    public void Dispose()
    {
        _jwtService.TokenUpdated -= JwtServiceOnTokenUpdated;
    }

    private void JwtServiceOnTokenUpdated(JwtSecurityToken securityToken)
    {
        var identity = new ClaimsIdentity(securityToken.Claims);
        var principal = new ClaimsPrincipal(identity);

        _authenticationState = new AuthenticationState(principal);

        NotifyAuthenticationStateChanged(Task.FromResult(_authenticationState));
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_authenticationState is null)
        {
            var identity = new ClaimsIdentity(_jwtService.SecurityToken.Claims);
            var principal = new ClaimsPrincipal(identity);

            _authenticationState = new AuthenticationState(principal);
            NotifyAuthenticationStateChanged(Task.FromResult(_authenticationState));
        }

        foreach (var claim in _authenticationState.User.Claims) Console.WriteLine($"{claim.Type}: {claim.Value}");

        return Task.FromResult(_authenticationState);
    }
}