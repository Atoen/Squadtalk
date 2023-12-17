using Shared.DTOs;

namespace Squadtalk.Services;

public class ConnectionKeyAccessor : IConnectionKeyAccessor<UserDto, string>
{
    public string GetKey(UserDto user)
    {
        return user.Id;
    }
}

public interface IConnectionKeyAccessor<in TUser, out TKey>
{
    TKey GetKey(TUser user);
}