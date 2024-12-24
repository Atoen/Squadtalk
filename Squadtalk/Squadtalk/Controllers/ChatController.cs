using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Extensions;
using Shared.Results;
using Shared.Routing;
using Squadtalk.Data;
using Squadtalk.Repositories;
using Squadtalk.Services;

namespace Squadtalk.Controllers;

[Authorize]
[ApiController]
[Route(Routes.Endpoints.ChatController)]
public class ChatController : ControllerBase
{
    private readonly MessageRepository _messageRepository;
    private readonly ChannelRepository _channelRepository;
    private readonly ChannelCreator _channelCreator;

    public ChatController(
        ChannelCreator channelCreator,
        MessageRepository messageRepository,
        ChannelRepository channelRepository)
    {
        _channelCreator = channelCreator;
        _messageRepository = messageRepository;
        _channelRepository = channelRepository;
    }

    [HttpGet(Routes.RelativeEndpoints.GetMessages)]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(ChannelId channelId, string? timestamp)
    {
        var userId = HttpContext.User.GetUserId();
        var userParticipates = await _channelRepository.UserParticipatesInChannelAsync(userId, channelId);
        if (!userParticipates)
        {
            return Forbid();
        }

        var messages = await _messageRepository.GetPageAsync(channelId, timestamp, HttpContext.RequestAborted);
        var dtos = messages.Select(x => x.ToDto());

        return Ok(dtos);
    }

    [HttpPost(Routes.RelativeEndpoints.CreateChannel)]
    public async Task<ActionResult<ChannelId>> CreateChannel(List<UserId> participantsId)
    {
        var userId = HttpContext.User.GetUserId();
        var channel = await _channelCreator.CreateChannelAsync(userId, participantsId);

        return channel is null
            ? BadRequest()
            : channel.Id;
    }

    [HttpGet(Routes.RelativeEndpoints.Friends)]
    public async Task<IEnumerable<UserDto>> GetUserFriends(
        [FromServices] FriendRepository friendRepository)
    {
        var userId = HttpContext.User.GetUserId();
        var friends = await friendRepository.GetUserFriendsAsync(userId);

        return friends.Select(x => x.ToDto());
    }

    [HttpGet(Routes.RelativeEndpoints.PendingFriendRequests)]
    public async Task<IEnumerable<PendingFriendRequestDto>> GetPendingFriendRequests(
        [FromServices] FriendRepository friendRepository)
    {
        var userId = HttpContext.User.GetUserId();
        var requests = await friendRepository.GetUserPendingFriendRequests(userId);

        return requests.Select(x => x.ToDto());
    }

    [HttpPost(Routes.RelativeEndpoints.SendFriendRequest)]
    public async Task<ActionResult> SendFriendRequest(
        FriendRequestDto friendRequestDto,
        [FromServices] FriendRepository friendRepository)
    {
        var userId = HttpContext.User.GetUserId();

        if (userId != friendRequestDto.RequestingUserId)
        {
            return Forbid();
        }

        var result = await friendRepository.AddFriendRequest(
            userId, friendRequestDto.RecipientUsername, HttpContext.RequestAborted);

        return result switch
        {
            FriendRequestResult.Success => Ok(result),
            FriendRequestResult.RecipientNotFound => NotFound(result),
            _ => BadRequest(result)
        };
    }

    [HttpPost(Routes.RelativeEndpoints.RespondToFriendRequest)]
    public async Task<ActionResult> RespondToFriendRequest(
        FriendRequestResponseDto friendRequestResponseDto,
        [FromServices] FriendRepository friendRepository)
    {
        var userId = HttpContext.User.GetUserId();

        if (userId != friendRequestResponseDto.RespondingUserId)
        {
            return Forbid();
        }

        var result = await friendRepository.RespondToFriendRequestAsync(
            userId,
            friendRequestResponseDto.FriendRequestId,
            friendRequestResponseDto.Accepted,
            HttpContext.RequestAborted);

        return result switch
        {
            FriendRequestResponseResult.Success => Ok(result),
            FriendRequestResponseResult.InvalidResponse => BadRequest(result),
            _ => Problem()
        };
    }

    [HttpPost(Routes.RelativeEndpoints.RemoveFriend)]
    public async Task<ActionResult> RemoveFriend(
        RemoveFriendDto removeFriendDto,
        [FromServices] FriendRepository friendRepository)
    {
        var userId = HttpContext.User.GetUserId();

        var result = await friendRepository.RemoveFriendAsync(
            userId, removeFriendDto.FriendId, HttpContext.RequestAborted);

        return result switch
        {
            RemoveFriendResult.Success => Ok(result),
            RemoveFriendResult.BadRequest => BadRequest(result),
            _ => Problem()
        };
    }
}
