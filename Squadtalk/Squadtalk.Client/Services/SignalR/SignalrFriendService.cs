using Microsoft.AspNetCore.SignalR.Client;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Enums;
using Shared.Results;
using Shared.Signalr;
using Shared.Signalr.Clients;
using Squadtalk.Client.Data;
using Squadtalk.Client.Services.SignalR.Interfaces;

namespace Squadtalk.Client.Services.SignalR;

internal sealed partial class SignalrService : ISignalrFriendService
{
    public event Action? UserStatusChanged;

    public event Action<PendingFriendRequestDto>? FriendRequestCreated;
    public event Action<FriendRequestId>? FriendRequestCancelled;
    public event Action<FriendRequestResponseDto>? FriendRequestResponded;

    public event Action<UserDto>? FriendAdded;
    public event Action<UserId>? FriendRemoved;

    public event Action<List<UserDto>>? FriendListReceived;
    public event Action<List<PendingFriendRequestDto>>? FriendRequestsReceived;
    public event Action<UserId, UserStatus>? FriendStatusChanged;

    public Task<NetworkResult<FriendRequestResult>> SendFriendRequestAsync(string recipientUsername)
    {
        var data = new FriendRequestDto { RecipientUsername = recipientUsername };
        return InvokeAsync<FriendRequestResult>(HubMethods.SendFriendRequest, data);
    }

    public Task<NetworkResult<bool>> CancelFriendRequestAsync(FriendRequestId friendRequestId)
    {
        var data = new CancelFriendRequestDto { RequestId = friendRequestId };
        return InvokeAsync<bool>(HubMethods.CancelFriendRequest, data);
    }

    public Task<NetworkResult<FriendRequestResponseResult>> RespondToFriendRequestAsync(FriendRequestId friendRequestId, bool isAccepted)
    {
        var data = new FriendRequestResponseDto
        {
            FriendRequestId = friendRequestId,
            Accepted = isAccepted
        };

        return InvokeAsync<FriendRequestResponseResult>(HubMethods.RespondToFriendRequest, data);
    }

    public Task<NetworkResult<RemoveFriendResult>> RemoveFriendAsync(UserId friendId)
    {
        var data = new RemoveFriendDto { FriendId = friendId };
        return InvokeAsync<RemoveFriendResult>(HubMethods.RemoveFriend, data);
    }

    public Task<NetworkResult<List<UserDto>>> GetFriendListAsync()
    {
        return InvokeAsync<List<UserDto>>(HubMethods.GetFriendList);
    }

    public Task<NetworkResult<List<PendingFriendRequestDto>>> GetFriendRequestsAsync()
    {
        return InvokeAsync<List<PendingFriendRequestDto>>(HubMethods.GetFriendRequests);
    }

    public Task<SignalrResult> SetStatusAsync(UserStatus status)
    {
        return SendAsync(HubMethods.ChangeStatus, status);
    }

    private void RegisterFriendHandlers()
    {
        _connection.On<PendingFriendRequestDto>(nameof(IFriendChatClient.FriendRequestCreated), friendRequest =>
            FriendRequestCreated?.Invoke(friendRequest));

        _connection.On<FriendRequestId>(nameof(IFriendChatClient.FriendRequestCancelled), friendRequestId =>
            FriendRequestCancelled?.Invoke(friendRequestId));

        _connection.On<FriendRequestResponseDto>(nameof(IFriendChatClient.FriendRequestResponded), response =>
            FriendRequestResponded?.Invoke(response));

        _connection.On<UserDto>(nameof(IFriendChatClient.FriendAdded), friend =>
            FriendAdded?.Invoke(friend));

        _connection.On<UserId>(nameof(IFriendChatClient.FriendRemoved), friendId =>
            FriendRemoved?.Invoke(friendId));

        _connection.On<UserId, UserStatus>(nameof(IFriendChatClient.FriendStatusChanged), (friendId, status) =>
            FriendStatusChanged?.Invoke(friendId, status));

        _connection.On<UserStatus>(nameof(IFriendChatClient.SelfStatusChanged), status =>
        {
            UserStatus = status;
            UserStatusChanged?.Invoke();
        });
    }
}
