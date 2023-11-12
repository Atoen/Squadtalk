namespace Squadtalk.Client.Models.Communication;

public sealed class User : ICommunicationTab
{
    public required string Username { get; init; }
    public required string Color { get; init; }
    public required string AvatarUrl { get; init; }
    public required Guid Id { get; init; }
    public UserStatus Status { get; set; }

    public string StatusColor => Status switch
    {
        UserStatus.Online => "#2ECC71",
        UserStatus.Away => "#FFD700",
        UserStatus.DoNotDisturb => "#FF6666",
        UserStatus.Offline => "gray",
        _ => throw new ArgumentOutOfRangeException()
    };

    public string StatusString => Status switch
    {
        UserStatus.Online => "Online",
        UserStatus.Away => "Away",
        UserStatus.DoNotDisturb => "Do not disturb",
        UserStatus.Offline => "Offline",
        _ => throw new ArgumentOutOfRangeException(nameof(Status))
    };

    public bool IsSelected { get; set; }
}