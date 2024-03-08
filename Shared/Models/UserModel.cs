using Shared.DTOs;
using Shared.Enums;

namespace Shared.Models;

public class UserModel
{
    public string Username { get; set; } = default!;
    
    public string Color { get; set; } = default!;
    public string AvatarUrl { get; set; } = default!;

    public string Id { get; set; } = default!;
    public UserStatus Status { get; set; }

    public static readonly List<UserModel> Models = [];

    public static UserModel GetOrCreate(UserDto dto)
    {
        if (Models.FirstOrDefault(x => x.Id == dto.Id) is { } model)
        {
            return model;
        }
        
        var newModel = new UserModel
        {
            Username = dto.Username,
            Id = dto.Id,
            Status = UserStatus.Offline,
            Color = "black",
            AvatarUrl = "user.png"
        };
        
        Models.Add(newModel);

        return newModel;
    }
}