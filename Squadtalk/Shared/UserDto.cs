namespace Squadtalk.Shared;

public record UserDto
{
    public required string Username { get; init; }
    public required Guid Id { get; init; }
}