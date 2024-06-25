using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;

namespace Shared.Models;

public class UserModel
{
    public string Username { get; set; } = default!;
    
    public string Color { get; set; } = default!;
    public string AvatarUrl { get; set; } = default!;

    public UserId Id { get; set; }
    public UserStatus Status { get; set; }

    public static readonly List<UserModel> Models = [];

    public static UserModel GetOrCreate(IChatUser user)
    {
        if (Models.FirstOrDefault(x => x.Id == user.Id) is { } model)
        {
            return model;
        }
        
        var newModel = new UserModel
        {
            Username = user.Username,
            Id = user.Id,
            Status = UserStatus.Offline,
            Color = "black",
            AvatarUrl = "user.png"
        };
        
        Models.Add(newModel);

        return newModel;
    }
}