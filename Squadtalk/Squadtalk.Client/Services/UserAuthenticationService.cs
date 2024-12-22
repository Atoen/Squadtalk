using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Shared.Data.TypedIds;
using Shared.Services;

namespace Squadtalk.Client.Services;

internal class UserAuthenticationService : AuthenticationStateProvider, IUserAuthenticationService
{
    private static readonly Task<AuthenticationState> DefaultUnauthenticatedTask =
        Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

    private readonly Task<AuthenticationState> _authenticationStateTask = DefaultUnauthenticatedTask;

    public ClaimsPrincipal User { get; }

    public bool IsAuthenticated => User is { Identity.IsAuthenticated: true };

    public UserId UserId { get; }

    public string Username { get; } = string.Empty;

    public string Email { get; } = string.Empty;

    public UserAuthenticationService(PersistentComponentState state)
    {
        if (!state.TryTakeFromJson<UserInfo>(nameof(UserInfo), out var userInfo) || userInfo is null)
        {
            User = new ClaimsPrincipal();
            return;
        }

        UserId = UserId.Parse(userInfo.UserId);
        Username = userInfo.Name;
        Email = userInfo.Email;

        User = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, userInfo.UserId),
            new Claim(ClaimTypes.Name, userInfo.Name),
            new Claim(ClaimTypes.Email, userInfo.Email)
        ], authenticationType: nameof(UserAuthenticationService)));

        _authenticationStateTask = Task.FromResult(new AuthenticationState(User));
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => _authenticationStateTask;
}
