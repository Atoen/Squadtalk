namespace Squadtalk.Client.Models;

public sealed class UserModel
{
    public required string Username { get; set; }
    public required string Color { get; set; }
    public required string AvatarUrl { get; set; }
    public UserStatus Status { get; set; }
    public required Guid Id { get; set; }

    public string StatusString => Status switch
    {
        UserStatus.Online => "Online",
        UserStatus.Away => "Away",
        UserStatus.DoNotDisturb => "Do not disturb",
        UserStatus.Offline => "Offline",
        _ => throw new ArgumentOutOfRangeException(nameof(Status))
    };

}