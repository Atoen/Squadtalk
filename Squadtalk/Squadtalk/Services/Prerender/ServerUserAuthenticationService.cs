using System.Security.Claims;
using Shared.Data.TypedIds;
using Shared.Extensions;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal class ServerUserAuthenticationService : IUserAuthenticationService
{
    private readonly ClaimsPrincipal _claimsPrincipal;

    public bool IsAuthenticated { get; }

    public UserId UserId { get; }

    public string Username { get; } = string.Empty;

    public string Email { get; } = string.Empty;

    public ServerUserAuthenticationService(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user is null)
        {
            _claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            IsAuthenticated = false;
            return;
        }

        _claimsPrincipal = user;
        IsAuthenticated = user.Identity is { IsAuthenticated: true };

        if (IsAuthenticated)
        {
            UserId = UserId.Parse(_claimsPrincipal.GetRequiredClaimValue(ClaimTypes.NameIdentifier));
            Username = _claimsPrincipal.GetRequiredClaimValue(ClaimTypes.Name);
            Email = user.GetRequiredClaimValue(ClaimTypes.Email);
        }
    }
}
