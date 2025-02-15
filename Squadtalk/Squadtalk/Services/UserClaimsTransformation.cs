using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Shared.Extensions;
using Squadtalk.Data;

namespace Squadtalk.Services;

internal sealed class UserClaimsTransformation(
    ApplicationDbContext dbContext,
    ILogger<UserClaimsTransformation> logger) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var userId = principal.GetUserId();
        if (userId is not { } id)
        {
            return new ClaimsPrincipal();
        }

        var user = await dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new { x.UserName, x.Email })
            .SingleOrDefaultAsync();

        if (user is null)
        {
            return new ClaimsPrincipal();
        }

        var principalUsername = principal.FindFirstValue(ClaimTypes.Name);
        var principalEmail = principal.FindFirstValue(ClaimTypes.Email);

        if (principalUsername == user.UserName && principalEmail == user.Email)
        {
            logger.LogDebug("principal data matches");
            return principal;
        }

        logger.LogDebug("updating principal data");

        var identity = new ClaimsIdentity(principal.Identity?.AuthenticationType);
        identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName!));
        identity.AddClaim(new Claim(ClaimTypes.Email, user.Email!));

        foreach (var claim in principal.Claims)
        {
            if (claim.Type != ClaimTypes.Name && claim.Type != ClaimTypes.Email)
            {
                identity.AddClaim(claim);
            }
        }

        return new ClaimsPrincipal(identity);
    }
}
