using Shared.DTOs;

namespace Shared.Services;

public interface ILiveKitService
{
    Task<RoomTokenDto?> CreateRoomTokenAsync(string username, string roomName);
}
