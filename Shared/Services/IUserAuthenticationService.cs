using System.Security.Claims;
using Shared.Data.TypedIds;

namespace Shared.Services;

public interface IUserAuthenticationService
{
    ClaimsPrincipal User { get; }

    bool IsAuthenticated { get; }

    UserId UserId { get; }

    string Username { get; }

    string Email { get; }
}
