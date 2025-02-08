using Shared.Data.TypedIds;

namespace Shared.Services;

public interface IUserAuthenticationService
{
    bool IsAuthenticated { get; }

    UserId UserId { get; }

    string Username { get; }

    string Email { get; }
}
