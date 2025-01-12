using Shared.Data.TypedIds;
using Shared.DTOs;
using Shared.DTOs.Chat;
using Squadtalk.Client.Data;

namespace Squadtalk.Client.Services.SignalR.Interfaces;

public interface ISignalrRTCService
{
    event Func<ChannelId, UserId, Task>? IncomingCall;
    event Func<ChannelId, UserDto, Task>? CallAccepted;
    event Func<UserDto, ChannelId, Task>? CallDeclined;
    event Func<ChannelId, Task>? CallEnded;
    event Func<string, Task>? CallFailed;

    Task<SignalrResult<RoomTokenDto?>> StartVoiceCallAsync(ChannelId id);

    Task<SignalrResult<RoomTokenDto?>> AcceptCallAsync(ChannelId id);

    Task<SignalrResult> DeclineCallAsync(ChannelId id);

    Task<SignalrResult<bool>> ChannelHasActiveCall(ChannelId id);
}
