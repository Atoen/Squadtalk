using Microsoft.AspNetCore.SignalR.Client;
using Shared.Data.TypedIds;
using Shared.DTOs;
using Shared.Extensions;
using Squadtalk.Client.SignalR;

namespace Squadtalk.Client.Services;

internal sealed partial class SignalrService : ISignalrRTCService
{
    public event Func<ChannelId, UserId, Task>? IncomingCall;
    public event Func<ChannelId, UserDto, Task>? CallAccepted;
    public event Func<UserDto, ChannelId, Task>? CallDeclined;
    public event Func<ChannelId, Task>? CallEnded;
    public event Func<string, Task>? CallFailed;

    Task<RoomTokenDto?> ISignalrRTCService.StartVoiceCallAsync(ChannelId id)
    {
        return _connection.InvokeAsync<RoomTokenDto?>("StartCall", id);
    }

    Task<RoomTokenDto?> ISignalrRTCService.AcceptCallAsync(ChannelId id)
    {
        return _connection.InvokeAsync<RoomTokenDto?>("AcceptCall", id);
    }

    Task ISignalrRTCService.DeclineCallAsync(ChannelId id)
    {
        return _connection.SendAsync("DeclineCall", id);
    }

    Task<bool> ISignalrRTCService.ChannelHasActiveCall(ChannelId id)
    {
        return _connection.InvokeAsync<bool>("ChannelHasActiveCall", id);
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
