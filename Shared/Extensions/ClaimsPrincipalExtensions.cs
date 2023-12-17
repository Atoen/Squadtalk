using System.Security.Claims;

namespace Shared.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetClaimValue(this ClaimsPrincipal user, string claimType)
    {
        var claim = user.Claims.FirstOrDefault(x => x.Type == claimType);
        return claim?.Value;
    }        

    public static string GetRequiredClaimValue(this ClaimsPrincipal user, string claimType)
    {
        var claim = user.Claims.FirstOrDefault(x => x.Type == claimType);

        if (claim == null) 
        {
            throw new Exception("Required Claim not found");
        }
        
        return claim.Value;
    }
}