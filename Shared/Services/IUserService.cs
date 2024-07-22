using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Shared.Models;

namespace Shared.Services;

public interface IUserService
{
    ClaimsPrincipal User { get; }

    [MemberNotNullWhen(true, nameof(UserModel))]
    bool IsAuthenticated { get; }

    UserModel? UserModel { get; }
}
