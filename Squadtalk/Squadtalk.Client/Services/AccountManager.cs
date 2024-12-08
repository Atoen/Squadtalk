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
            var response = await accountApi.Login(loginDto).ConfigureAwait(false);
            return response.Successful switch
            {
                true => new LoginResult.Success(),
                false => new LoginResult.Fail()
            };
        }
        catch
        {
            return new LoginResult.NetworkError();
        }
    }

    public async Task<RegisterResult> RegisterAsync(UserRegisterDto registerDto)
    {
        try
        {
            var response = await accountApi.Register(registerDto).ConfigureAwait(false);
            return response.Type switch
            {
                RegisterResultDto.ResultType.Success => new RegisterResult.Success(),
                RegisterResultDto.ResultType.EmailInUse => new RegisterResult.EmailInUse(),
                RegisterResultDto.ResultType.UsernameInUse => new RegisterResult.UsernameInUse(),
                RegisterResultDto.ResultType.FailedToCreateAccount => new RegisterResult.FailedToCreateAccount(),
                _ => new RegisterResult.FailedToCreateAccount()
            };
        }
        catch
        {
            return new RegisterResult.NetworkError();
        }
    }

    public async Task<ForgotPasswordResult> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        try
        {
            var response = await accountApi.ForgotPassword(forgotPasswordDto).ConfigureAwait(false);
            return response.IsSuccessful switch
            {
                true => new ForgotPasswordResult.Success(),
                false => new ForgotPasswordResult.Fail()
            };

        }
        catch
        {
            return new ForgotPasswordResult.NetworkError();
        }
    }

    public async Task<ResetPasswordResult> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        try
        {
            var response = await accountApi.ResetPassword(resetPasswordDto).ConfigureAwait(false);
            return response.Successful switch
            {
                true => new ResetPasswordResult.Success(),
                false => new ResetPasswordResult.Fail()
            };
        }
        catch
        {
            return new ResetPasswordResult.NetworkError();
        }
    }

    public async Task<ChangeUsernameResult> ChangeUsernameAsync(ChangeUsernameDto changeUsernameDto)
    {
        try
        {
            var response = await accountApi.ChangeUsername(changeUsernameDto).ConfigureAwait(false);
            return response.Type switch
            {
                ChangeUsernameResultDto.ResultType.Success => new ChangeUsernameResult.Success(response.NewUsername!),
                ChangeUsernameResultDto.ResultType.NotChanged => new ChangeUsernameResult.NotChanged(),
                ChangeUsernameResultDto.ResultType.NotFound => new ChangeUsernameResult.NotFound(),
                ChangeUsernameResultDto.ResultType.Unauthorized => new ChangeUsernameResult.Unauthorized(),
                ChangeUsernameResultDto.ResultType.UsernameInUse => new ChangeUsernameResult.UsernameInUse(),
                ChangeUsernameResultDto.ResultType.FailedToChange => new ChangeUsernameResult.FailedToChange(),
                _ => new ChangeUsernameResult.FailedToChange()
            };
        }
        catch
        {
            return new ChangeUsernameResult.NetworkError();
        }
    }

    public async Task<ChangeEmailResult> ChangeEmailAsync(ChangeEmailDto changeEmailDto)
    {
        try
        {
            var response = await accountApi.ChangeEmail(changeEmailDto).ConfigureAwait(false);
            return response.Type switch
            {
                ChangeEmailResultDto.ResultType.ConfirmationSent => new ChangeEmailResult.ConfirmationSent(),
                ChangeEmailResultDto.ResultType.NotChanged => new ChangeEmailResult.NotChanged(),
                ChangeEmailResultDto.ResultType.NotFound => new ChangeEmailResult.NotFound(),
                ChangeEmailResultDto.ResultType.EmailInUse => new ChangeEmailResult.EmailInUse(),
                ChangeEmailResultDto.ResultType.Unauthorized => new ChangeEmailResult.Unauthorized(),
                _ => new ChangeEmailResult.FailedToChange()
            };
        }
        catch
        {
            return new ChangeEmailResult.NetworkError();
        }
    }

    public async Task<ChangePasswordResult> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
    {
        try
        {
            var response = await accountApi.ChangePassword(changePasswordDto).ConfigureAwait(false);
            return response.Successful switch
            {
                true => new ChangePasswordResult.Success(),
                false => new ChangePasswordResult.Fail()
            };
        }
        catch
        {
            return new ChangePasswordResult.NetworkError();
        }
    }
}
