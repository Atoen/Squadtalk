using Microsoft.AspNetCore.SignalR;
using Shared.DTOs.Chat;
using Shared.Extensions;
using Shared.Results;
using Shared.Signalr;
using Squadtalk.Data;
using Squadtalk.Repositories;

namespace Squadtalk.Signalr.Hubs;

partial class AppHub
{
    [HubMethodName(HubMethods.SendFriendRequest)]
    public async Task<FriendRequestResultDto> SendFriendRequest(
        FriendRequestDto friendRequest, FriendRepository friendRepository)
    {
        var userId = Context.User!.GetUserId();

        var result = await friendRepository.AddFriendRequestAsync(
            userId, friendRequest.RecipientUsername, Context.ConnectionAborted);

        if (result is SendFriendRequestResult.Success success)
        {
            var request = await friendRepository.FindFriendRequestByIdAsync(success.RequestId);
            var friendRequestDto = request?.ToDto();

            if (friendRequestDto is not null)
            {
                var recipientId = friendRequestDto.Recipient.Id;
                await Clients.User(recipientId.ToString()).FriendRequestReceived(friendRequestDto);
            }

            return new FriendRequestResultDto
            {
                Status = FriendRequestResult.Success,
                FriendRequest = friendRequestDto
            };
        }

        return new FriendRequestResultDto
        {
            Status = result.Value,
            FriendRequest = null
        };
    }

    [HubMethodName(HubMethods.CancelFriendRequest)]
    public async Task<bool> CancelFriendRequest(
        CancelFriendRequestDto cancelFriendRequestDto, FriendRepository friendRepository)
    {
        var userId = Context.User!.GetUserId();
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
        var userId = Context.User!.GetUserId();

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

                var respondingUserId = userId.ToString();
                var requestingUserId = accepted.RequesterId.ToString();

                if (accepted.OtherWayRequestId is { } otherWayRequestId)
                {
                    await Clients.Users(respondingUserId, requestingUserId).FriendRequestCancelled(otherWayRequestId);
                }

                await Clients.User(requestingUserId).FriendRequestResponded(requestResponseDto);

                await Clients.User(friendship.User1.Id.ToString()).FriendAdded(friendship.User2.ToDto());
                await Clients.User(friendship.User2.Id.ToString()).FriendAdded(friendship.User1.ToDto());
                break;
            }

            case RespondToFriendRequestResult.Rejected rejected:
                await Clients.User(rejected.RequesterId.ToString()).FriendRequestResponded(requestResponseDto);
                break;
        }

        return result.Value;
    }

    [HubMethodName(HubMethods.RemoveFriend)]
    public async Task<RemoveFriendResult> RemoveFriend(
        RemoveFriendDto removeFriendDto, FriendRepository friendRepository)
    {
        var userId = Context.User!.GetUserId();

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
}
