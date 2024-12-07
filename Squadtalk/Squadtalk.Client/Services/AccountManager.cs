using Shared.Data.Results;
using Shared.DTOs.Account;
using Shared.DTOs.Account.Results;
using Shared.Services;
using Squadtalk.Client.Network;

namespace Squadtalk.Client.Services;

internal class AccountManager(IAccountApi accountApi) : IAccountManager
{
    public async Task<LoginResult> LoginAsync(UserLoginDto loginDto)
    {
        try
        {
            var response = await accountApi.Login(loginDto);
            return response.Successful ? LoginResult.Success : LoginResult.Fail;
        }
        catch
        {
            return LoginResult.NetworkError;
        }
    }

    public async Task<RegisterResult> RegisterAsync(UserRegisterDto registerDto)
    {
        try
        {
            var response = await accountApi.Register(registerDto);
            return response.Type switch
            {
                RegisterResultDto.ResultType.Success => RegisterResult.Success,
                RegisterResultDto.ResultType.EmailInUse => RegisterResult.EmailInUse,
                RegisterResultDto.ResultType.UsernameInUse => RegisterResult.UsernameInUse,
                RegisterResultDto.ResultType.FailedToCreateAccount => RegisterResult.FailedToCreateAccount,
                _ => RegisterResult.FailedToCreateAccount
            };
        }
        catch
        {
            return RegisterResult.NetworkError;
        }
    }
}
