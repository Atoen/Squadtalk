using Shared.Data.TypedIds;
using Shared.DTOs;
using Shared.DTOs.Chat;
using Squadtalk.Client.Data;

namespace Squadtalk.Client.Services.SignalR.Interfaces;

public interface ISignalrRTCService
{
    event Func<GroupId, UserId, Task>? IncomingCall;
    event Func<GroupId, UserDto, Task>? CallAccepted;
    event Func<UserDto, GroupId, Task>? CallDeclined;
    event Func<GroupId, Task>? CallEnded;
    event Func<string, Task>? CallFailed;

    Task<NetworkResult<RoomTokenDto?>> StartVoiceCallAsync(GroupId id);

    Task<NetworkResult<RoomTokenDto?>> AcceptCallAsync(GroupId id);

    Task<NetworkResult> DeclineCallAsync(GroupId id);

    Task<NetworkResult<bool>> GroupHasActiveCall(GroupId id);
}
