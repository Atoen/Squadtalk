using Shared.Data.TypedIds;
using Shared.DTOs;

namespace Shared.Services;

public interface ISignalrVoiceService
{
    event Func<ChannelId, UserId, Task>? IncomingCall;
    event Func<ChannelId, UserDto, Task>? CallAccepted;
    event Func<UserDto, ChannelId, Task>? CallDeclined;
    event Func<ChannelId, Task>? CallEnded;
    event Func<string, Task>? CallFailed;
    
    Task<RoomTokenDto?> StartVoiceCallAsync(ChannelId id);

    Task<RoomTokenDto?> AcceptCallAsync(ChannelId id);
    
    Task DeclineCallAsync(ChannelId id);
}
