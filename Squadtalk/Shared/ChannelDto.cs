namespace Squadtalk.Shared;

public record ChannelDto
{
    public Guid Id { get; set; }
    public List<UserDto> Participants { get; set; } = null!;
}