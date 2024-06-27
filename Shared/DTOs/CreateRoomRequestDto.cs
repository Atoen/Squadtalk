namespace Shared.DTOs;

public class CreateRoomRequestDto
{
    public string Username { get; set; } = default!;

    public string RoomName { get; set; } = default!;
}
