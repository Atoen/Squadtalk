using Microsoft.AspNetCore.SignalR;
using Shared.DTOs.Chat;
using Shared.Enums;
using Shared.Results;
using Shared.Signalr;
using Squadtalk.Data;
using Squadtalk.Repositories;

namespace Squadtalk.Signalr.Hubs;

partial class AppHub
{
    [HubMethodName(HubMethods.SendFriendRequest)]
    public async Task<FriendRequestResult> SendFriendRequest(
        FriendRequestDto friendRequest, FriendRepository friendRepository)
    {
        var userId = UserId;

        var result = await friendRepository.AddFriendRequestAsync(
            userId, friendRequest.RecipientUsername, Context.ConnectionAborted);

        if (result is SendFriendRequestResult.Success success)
        {
            var request = await friendRepository.FindFriendRequestByIdAsync(success.RequestId);
            if (request is null)
            {
                return FriendRequestResult.Error;
            }

            var recipientId = request.Recipient.Id.ToString();
            var requesterId = userId.ToString();
            await Clients.Users(requesterId, recipientId).FriendRequestCreated(request.ToDto());
        }

        return result.Value;
    }

    [HubMethodName(HubMethods.CancelFriendRequest)]
    public async Task<bool> CancelFriendRequest(
        CancelFriendRequestDto cancelFriendRequestDto, FriendRepository friendRepository)
    {
        var userId = UserId;
        var requestId = cancelFriendRequestDto.RequestId;

        var cancelledRequest = await friendRepository.CancelFriendRequestAsync(
            userId, cancelFriendRequestDto.RequestId, Context.ConnectionAborted);

        if (cancelledRequest is null)
        {
            return false;
        }

        var userId1 = cancelledRequest.Recipient.Id.ToString();
        var userId2 = cancelledRequest.Requester.Id.ToString();

        await Clients.Users(userId1, userId2).FriendRequestCancelled(requestId);

        return true;
    }

    [HubMethodName(HubMethods.RespondToFriendRequest)]
    public async Task<FriendRequestResponseResult> RespondToFriendRequest(
        FriendRequestResponseDto requestResponseDto, FriendRepository friendRepository)
    {
        var userId = UserId;

        var result = await friendRepository.RespondToFriendRequestAsync(
            userId,
            requestResponseDto.FriendRequestId,
            requestResponseDto.Accepted,
            Context.ConnectionAborted);

        switch (result)
        {
            case RespondToFriendRequestResult.Accepted accepted:
            {
                var friendship = await friendRepository.FindFriendshipById(accepted.FriendshipId);
                if (friendship is null)
                {
                    return FriendRequestResponseResult.Error;
                }

                var userIds = new[] { userId.ToString(), accepted.RequesterId.ToString() };

                if (accepted.OtherWayRequestId is { } otherWayRequestId)
                {
                    await Clients.Users(userIds).FriendRequestCancelled(otherWayRequestId);
                }

                await Clients.Users(userIds).FriendRequestResponded(requestResponseDto);

                await Clients.User(friendship.User1.Id.ToString()).FriendAdded(friendship.User2.ToDto());
                await Clients.User(friendship.User2.Id.ToString()).FriendAdded(friendship.User1.ToDto());
                break;
            }

            case RespondToFriendRequestResult.Rejected rejected:
            {
                var userIds = new[] { userId.ToString(), rejected.RequesterId.ToString() };
                await Clients.Users(userIds).FriendRequestResponded(requestResponseDto);
                break;
            }
        }

        return result.Value;
    }

    [HubMethodName(HubMethods.RemoveFriend)]
    public async Task<RemoveFriendResult> RemoveFriend(
        RemoveFriendDto removeFriendDto, FriendRepository friendRepository)
    {
        var userId = UserId;

        var result = await friendRepository.RemoveFriendAsync(
            userId, removeFriendDto.FriendId, Context.ConnectionAborted);

        if (result == RemoveFriendResult.Success)
        {
            var otherUserId = removeFriendDto.FriendId;

            await Clients.User(userId.ToString()).FriendRemoved(otherUserId);
            await Clients.User(otherUserId.ToString()).FriendRemoved(userId);
        }

        return result;
    }

    [HubMethodName(HubMethods.GetFriendList)]
    public async Task<List<UserDto>> GetFriendList(FriendRepository friendRepository)
    {
        var friends = await friendRepository.GetUserFriendsAsync(UserId);
        var friendIds = friends.Select(x => x.Id);

        var friendStatuses = await _connectionManager.GetUsersStatusAsync(friendIds);

        var dtos = friends.Select(friend =>
        {
            var friendStatus = friendStatuses.GetValueOrDefault(friend.Id, UserStatus.Unknown);
            return friend.ToDto(friendStatus);
        });

        return dtos.ToList();
    }

    [HubMethodName(HubMethods.GetFriendRequests)]
    public async Task<List<PendingFriendRequestDto>> GetFriendRequests(FriendRepository friendRepository)
    {
        var requests = await friendRepository.GetUserPendingFriendRequests(UserId);
        return requests.Select(x => x.ToDto()).ToList();
    }

    [HubMethodName(HubMethods.ChangeStatus)]
    public async Task ChangeStatus(UserStatus newStatus, FriendRepository friendRepository)
    {
        var userId = UserId;
        var (statusChanged, currentStatus) = await _connectionManager.SetUserStatusAsync(userId, newStatus);

        _logger.LogInformation("User {Username} status changed: {Changed}, now: {Current}", userId, statusChanged, currentStatus);

        if (!statusChanged) return;

        var friendsId = await friendRepository.GetUserFriendIdsAsync(userId);
        var idStrings = friendsId.Select(x => x.ToString());

        await Clients.User(userId.ToString()).SelfStatusChanged(currentStatus);

        await Clients.Users(idStrings).FriendStatusChanged(userId, currentStatus);
    }

    [HubMethodName(HubMethods.GetSelfStatus)]
    public async Task<UserStatus> GetSelfStatus()
    {
        return await _connectionManager.GetUserStatusAsync(UserId);
    }
}
