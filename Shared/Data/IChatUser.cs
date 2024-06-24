using Shared.Data.TypedIds;

namespace Shared.Data;

public interface IChatUser
{
    string Username { get; }
    
    UserId Id { get; }
}
