using System.Security.Claims;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Services;

public class ServerUserService : IUserService
{

    public ClaimsPrincipal User { get; }
    public bool IsAuthenticated { get; }
    public UserModel? UserModel { get; }
}
