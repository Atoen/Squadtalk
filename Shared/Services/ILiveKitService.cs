using Shared.Data.TypedIds;
using Shared.DTOs;

namespace Shared.Services;

public interface ILiveKitService
{
    Task<RoomTokenDto?> CreateRoomTokenAsync(ChannelId channelId);
}
