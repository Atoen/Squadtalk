using Refit;
using Shared.DTOs.Account;
using Shared.DTOs.Account.Results;
using Shared.Routing;

namespace Squadtalk.Client.Network;

internal interface IAccountApi
{
    [Post(Routes.Endpoints.Login)]
    Task<LoginResultDto> Login(UserLoginDto loginDto);

    [Post(Routes.Endpoints.Register)]
    Task<RegisterResultDto> Register(UserRegisterDto registerDto);
}
