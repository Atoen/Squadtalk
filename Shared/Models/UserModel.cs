using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;

namespace Shared.Models;

public class UserModel : IStatus
{
    public string Username { get; set; } = default!;

    public string AvatarUrl { get; set; } = default!;

    public UserId Id { get; set; }

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
}

