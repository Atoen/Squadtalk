using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;

namespace Shared.Models;

public class UserModel : IEquatable<UserModel>, IStatus
{
    public string Username { get; set; } = default!;
    
    public string Color { get; set; } = default!;
    public string AvatarUrl { get; set; } = default!;

    public UserId Id { get; set; }
    public UserStatus Status { get; set; }

    public static readonly List<UserModel> Models = [];

    public static UserModel? Get(UserId userId) => Models.SingleOrDefault(x => x.Id == userId);

    public static UserModel GetOrCreate(string username, UserId userId)
    {
        if (Models.FirstOrDefault(x => x.Id == userId) is { } model)
        {
            return model;
        }

        var newModel = new UserModel
        {
            Username = username,
            Id = userId,
            Status = UserStatus.Offline,
            Color = "black",
            AvatarUrl = "user.png"
        };

        Models.Add(newModel);

        return newModel;
    }

    public static UserModel GetOrCreate(IChatUser user)
    {
        return GetOrCreate(user.Username, user.Id);
    }

    public bool Equals(UserModel? other) => Id == other?.Id;
}