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
    public event Func<ChannelId, UserId, Task>? IncomingCall;
    public event Func<ChannelId, UserDto, Task>? CallAccepted;
    public event Func<UserDto, ChannelId, Task>? CallDeclined;
    public event Func<ChannelId, Task>? CallEnded;
    public event Func<string, Task>? CallFailed;

    public Task<SignalrResult<RoomTokenDto?>> StartVoiceCallAsync(ChannelId id)
    {
        return InvokeAsync<RoomTokenDto?>("StartCall", id);
    }

    public Task<SignalrResult<RoomTokenDto?>> AcceptCallAsync(ChannelId id)
    {
        return InvokeAsync<RoomTokenDto?>("AcceptCall", id);
    }

    public Task<SignalrResult> DeclineCallAsync(ChannelId id)
    {
        return SendAsync("DeclineCall", id);
    }

    public Task<SignalrResult<bool>> ChannelHasActiveCall(ChannelId id)
    {
        return InvokeAsync<bool>("ChannelHasActiveCall", id);
    }

    private void RegisterRTCHandlers()
    {
        _connection.On<ChannelId, UserId>("IncomingCall", (channelId, initiatorId) =>
            IncomingCall.TryInvoke(channelId, initiatorId));

        _connection.On<ChannelId, UserDto>("CallAccepted", (channelId, accepting) =>
            CallAccepted.TryInvoke(channelId, accepting));

        _connection.On<UserDto, ChannelId>("CallDeclined", (user, channelId) =>
            CallDeclined.TryInvoke(user, channelId));

        _connection.On<ChannelId>("CallEnded", channelId =>
            CallEnded.TryInvoke(channelId));

        _connection.On<string>("CallFailed", reason =>
            CallFailed.TryInvoke(reason));
    }
}
