using Shared.Data.Results;
using Shared.DTOs.Account;

namespace Shared.Services;

public interface IAccountManager
{
    Task<LoginResult> LoginAsync(UserLoginDto loginDto);

    Task<RegisterResult> RegisterAsync(UserRegisterDto registerDto);
}
