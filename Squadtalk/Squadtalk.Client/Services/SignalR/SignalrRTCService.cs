using Microsoft.AspNetCore.SignalR.Client;
using Shared.Data.TypedIds;
using Shared.DTOs;
using Shared.DTOs.Chat;
using Shared.Extensions;
using Squadtalk.Client.Data;
using Squadtalk.Client.Services.SignalR.Interfaces;

namespace Squadtalk.Client.Services.SignalR;

internal sealed partial class SignalrService : ISignalrRTCService
{
    public event Func<GroupId, UserId, Task>? IncomingCall;
    public event Func<GroupId, UserDto, Task>? CallAccepted;
    public event Func<UserDto, GroupId, Task>? CallDeclined;
    public event Func<GroupId, Task>? CallEnded;
    public event Func<string, Task>? CallFailed;

    public Task<NetworkResult<RoomTokenDto?>> StartVoiceCallAsync(GroupId id)
    {
        return InvokeAsync<RoomTokenDto?>("StartCall", id);
    }

    public Task<NetworkResult<RoomTokenDto?>> AcceptCallAsync(GroupId id)
    {
        return InvokeAsync<RoomTokenDto?>("AcceptCall", id);
    }

    public Task<SignalrResult> DeclineCallAsync(GroupId id)
    {
        return SendAsync("DeclineCall", id);
    }

    public Task<NetworkResult<bool>> ChannelHasActiveCall(GroupId id)
    {
        return InvokeAsync<bool>("ChannelHasActiveCall", id);
    }

    private void RegisterRTCHandlers()
    {
        _connection.On<GroupId, UserId>("IncomingCall", (channelId, initiatorId) =>
            IncomingCall.TryInvoke(channelId, initiatorId));

        _connection.On<GroupId, UserDto>("CallAccepted", (channelId, accepting) =>
            CallAccepted.TryInvoke(channelId, accepting));

        _connection.On<UserDto, GroupId>("CallDeclined", (user, channelId) =>
            CallDeclined.TryInvoke(user, channelId));

        _connection.On<GroupId>("CallEnded", channelId =>
            CallEnded.TryInvoke(channelId));

        _connection.On<string>("CallFailed", reason =>
            CallFailed.TryInvoke(reason));
    }
}
