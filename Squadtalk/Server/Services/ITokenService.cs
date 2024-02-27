using System.Security.Claims;
using Squadtalk.Server.Models;

namespace Squadtalk.Server.Services;

public interface ITokenService
{
    TimeSpan AuthTokenTimeSpan { get; }
    TimeSpan RefreshTokenTimeSpan { get; }

    bool VerifyRefreshToken(User user, string token);
    RefreshToken CreateRefreshToken();
    string CreateAuthToken(params Claim[] claims);
    Task<bool> RevokeRefreshToken(User user, string token);
    Task RevokeAllRefreshTokens(User user);
}