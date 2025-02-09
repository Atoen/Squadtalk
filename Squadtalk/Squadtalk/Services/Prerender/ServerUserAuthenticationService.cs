using System.Security.Claims;
using Shared.Data.TypedIds;
using Shared.Extensions;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal class ServerUserAuthenticationService : IUserAuthenticationService
{
    event Action? IUserAuthenticationService.LocalUsernameChanged { add { } remove { } }

    public bool IsAuthenticated { get; }

    public UserId UserId { get; }

    public string Username { get; } = string.Empty;

    public string Email { get; } = string.Empty;

    public ServerUserAuthenticationService(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user is null)
        {
            IsAuthenticated = false;
            return;
        }

        IsAuthenticated = user.Identity is { IsAuthenticated: true };

        if (IsAuthenticated)
        {
            UserId = UserId.Parse(user.GetRequiredClaimValue(ClaimTypes.NameIdentifier));
            Username = user.GetRequiredClaimValue(ClaimTypes.Name);
            Email = user.GetRequiredClaimValue(ClaimTypes.Email);
        }
    }
}
