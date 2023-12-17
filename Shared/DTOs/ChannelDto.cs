namespace Shared.DTOs;

public class ChannelDto
{
    public string Id { get; set; } = default!;

    public List<UserDto> Participants { get; set; } = default!;
}