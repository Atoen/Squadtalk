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

    Task<SignalrResult<RoomTokenDto?>> StartVoiceCallAsync(GroupId id);

    Task<SignalrResult<RoomTokenDto?>> AcceptCallAsync(GroupId id);

    Task<SignalrResult> DeclineCallAsync(GroupId id);

    Task<SignalrResult<bool>> ChannelHasActiveCall(GroupId id);
}
