using System.Security.Claims;
using Shared.Data.TypedIds;
using Shared.Extensions;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

public class ServerUserAuthenticationService : IUserAuthenticationService
{
    public ClaimsPrincipal User { get; }

    public bool IsAuthenticated { get; }

    public UserId UserId { get; }

    public string Username { get; } = string.Empty;

    public ServerUserAuthenticationService(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user is null)
        {
            User = new ClaimsPrincipal(new ClaimsIdentity());
            IsAuthenticated = false;
            return;
        }

        User = user;
        IsAuthenticated = user.Identity is { IsAuthenticated: true };

        if (IsAuthenticated)
        {
            UserId = UserId.Parse(User.GetRequiredClaimValue(ClaimTypes.NameIdentifier));
            Username = User.GetRequiredClaimValue(ClaimTypes.Name);
        }
    }
}
