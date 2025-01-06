using Microsoft.AspNetCore.SignalR.Client;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Results;
using Shared.Signalr;
using Shared.Signalr.Clients;

namespace Squadtalk.Client.Services.SignalR;

internal sealed partial class SignalrService
{
    public event Action<PendingFriendRequestDto>? FriendRequestCreated;
    public event Action<FriendRequestId>? FriendRequestCancelled;
    public event Action<FriendRequestResponseDto>? FriendRequestResponded;

    public event Action<UserDto>? FriendAdded;
    public event Action<UserId>? FriendRemoved;

    public async Task<FriendRequestResult> SendFriendRequestAsync(string recipientUsername)
    {
        var data = new FriendRequestDto { RecipientUsername = recipientUsername };
        return await _connection.InvokeAsync<FriendRequestResult>(HubMethods.SendFriendRequest, data);
    }

    public async Task<bool> CancelFriendRequestAsync(FriendRequestId friendRequestId)
    {
        var data = new CancelFriendRequestDto { RequestId = friendRequestId };
        return await _connection.InvokeAsync<bool>(HubMethods.CancelFriendRequest, data);
    }

    public async Task<FriendRequestResponseResult> RespondToFriendRequestAsync(FriendRequestId friendRequestId, bool isAccepted)
    {
        var data = new FriendRequestResponseDto
        {
            FriendRequestId = friendRequestId,
            Accepted = isAccepted
        };

        return await _connection.InvokeAsync<FriendRequestResponseResult>(HubMethods.RespondToFriendRequest, data);
    }

    public async Task<RemoveFriendResult> RemoveFriendAsync(UserId friendId)
    {
        var data = new RemoveFriendDto { FriendId = friendId };
        return await _connection.InvokeAsync<RemoveFriendResult>(HubMethods.RemoveFriend, data);
    }

    private void RegisterFriendHandlers()
    {
        _connection.On<PendingFriendRequestDto>(nameof(IChatClient.FriendRequestCreated), friendRequest =>
            FriendRequestCreated?.Invoke(friendRequest));

        _connection.On<FriendRequestId>(nameof(IChatClient.FriendRequestCancelled), friendRequestId =>
            FriendRequestCancelled?.Invoke(friendRequestId));

        _connection.On<FriendRequestResponseDto>(nameof(IChatClient.FriendRequestResponded), response =>
            FriendRequestResponded?.Invoke(response));

        _connection.On<UserDto>(nameof(IChatClient.FriendAdded), friend =>
            FriendAdded?.Invoke(friend));

        _connection.On<UserId>(nameof(IChatClient.FriendRemoved), friendId =>
            FriendRemoved?.Invoke(friendId));
    }
}
