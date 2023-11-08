using Squadtalk.Shared;

namespace Squadtalk.Server.Models;

public class User
{
    public required Guid Id { get; set; }
    public required string Username { get; set; }
    public required byte[] Salt { get; set; }
    public required string PasswordHash { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; } = null!;
    public List<Channel> Channels { get; set; } = null!;

    public UserDto ToDto()
    {
        return new UserDto
        {
            Username = Username,
            Id = Id
        };
    }
}
