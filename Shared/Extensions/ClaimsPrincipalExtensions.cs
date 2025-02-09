using System.Security.Claims;
using Shared.Data.TypedIds;

namespace Shared.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetClaimValue(this ClaimsPrincipal principal, string claimType)
    {
        var claim = principal.Claims.FirstOrDefault(x => x.Type == claimType);
        return claim?.Value;
    }

    public static string GetRequiredClaimValue(this ClaimsPrincipal principal, string claimType)
    {
        var claim = principal.Claims.FirstOrDefault(x => x.Type == claimType);
        if (claim is null)
        {
            throw new Exception("Required Claim not found");
        }

        return claim.Value;
    }

    public static UserId GetRequiredUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.GetRequiredClaimValue(ClaimTypes.NameIdentifier);

        return UserId.Parse(claim);
    }

    public static UserId? GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.GetClaimValue(ClaimTypes.NameIdentifier);

        return claim is null ? null : UserId.Parse(claim);
    }
}