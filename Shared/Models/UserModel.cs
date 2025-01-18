using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;

namespace Shared.Models;

public class UserModel : IStatus, IEquatable<UserModel?>
{
    public string Username { get; set; } = default!;

    public string AvatarUrl { get; set; } = default!;

    public UserId Id { get; init; }

    public UserStatus Status { get; set; }

    public static UserModel Create(IChatUser chatUser)
    {
        return new UserModel
        {
            Username = chatUser.Username,
            Id = chatUser.Id,
            AvatarUrl = "user.png",
            Status = chatUser.Status
        };
    }

    public bool Equals(UserModel? other)
    {
        if (other is null) return false;

        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is UserModel userModel)
        {
            return Equals(userModel);
        }

        return false;
    }

    public override int GetHashCode() => Id.GetHashCode();
}

