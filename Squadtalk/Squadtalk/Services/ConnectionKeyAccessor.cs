using Squadtalk.Data;

namespace Squadtalk.Services;

public class ConnectionKeyAccessor : IConnectionKeyAccessor<ApplicationUser, string>
{
    public string GetKey(ApplicationUser user)
    {
        return user.Id;
    }
}

public interface IConnectionKeyAccessor<in TUser, out TKey>
{
    TKey GetKey(TUser user);
}