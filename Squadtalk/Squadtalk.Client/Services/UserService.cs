using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Shared.Data.TypedIds;
using Shared.Extensions;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Client.Services;

public class UserService : IUserService
{
    public UserService(AuthenticationStateProvider authenticationStateProvider)
    {
        var authenticationState = authenticationStateProvider.GetAuthenticationStateAsync()
            .GetAwaiter()
            .GetResult();

        User = authenticationState.User;

        IsAuthenticated = User.Identity is { IsAuthenticated: true };

        var id = UserId.Parse(User.GetRequiredClaimValue(ClaimTypes.NameIdentifier));
        var username = User.GetRequiredClaimValue(ClaimTypes.Name);

        UserModel = UserModel.GetOrCreate(username, id);
    }

    public ClaimsPrincipal User { get; }

    public bool IsAuthenticated { get; }

    public UserModel? UserModel { get; }
}
