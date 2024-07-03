using Shared.Data.TypedIds;

namespace Shared.DTOs;

public class CreateRoomRequestDto
{
    public ChannelId ChannelId { get; set; } = default!;
}
