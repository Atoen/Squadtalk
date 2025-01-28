using Shared.Data.TypedIds;
using Shared.Enums;

namespace Shared.Data;

public interface IChatUser
{
    string Username { get; }

    UserId Id { get; }

    UserStatus Status { get; }
}
