using Shared.Data.Results;
using Shared.DTOs.Account;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

public class AccountManager : IAccountManager
{
    public Task<LoginResult> LoginAsync(UserLoginDto loginDto) => throw new NotImplementedException();
    public Task<RegisterResult> RegisterAsync(UserRegisterDto registerDto) => throw new NotImplementedException();
}
