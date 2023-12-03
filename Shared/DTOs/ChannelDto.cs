namespace Shared.DTOs;

public class ChannelDto
{
    public string Id { get; set; } = default!;

    public List<UserDto> UserDtos { get; set; } = default!;
}