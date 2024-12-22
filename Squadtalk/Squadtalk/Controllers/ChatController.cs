using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.DTOs.Chat.Results;
using Shared.Extensions;
using Shared.Routing;
using Squadtalk.Data;
using Squadtalk.Data.Entities;
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
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly ChannelCreator _channelCreator;

    public ChatController(
        ChannelCreator channelCreator,
        MessageRepository messageRepository,
        ChannelRepository channelRepository,
        ApplicationDbContext applicationDbContext)
    {
        _channelCreator = channelCreator;
        _messageRepository = messageRepository;
        _channelRepository = channelRepository;
        _applicationDbContext = applicationDbContext;
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

    [HttpPost(Routes.RelativeEndpoints.SendFriendRequest)]
    public async Task<ActionResult<FriendRequestResultDto>> SendFriendRequest(FriendRequestDto friendRequestDto)
    {
        var userId = HttpContext.User.GetUserId();

        var recipient = await _applicationDbContext.Users
            .FirstOrDefaultAsync(x => x.UserName == friendRequestDto.RecipientUsername);

        if (recipient is null)
        {
            return BadRequest("Null recipient");
        }

        var requestExists = await _applicationDbContext.FriendRequests
            .AnyAsync(x => x.Requester.Id == userId && x.Recipient.Id == recipient.Id);

        if (requestExists)
        {
            return BadRequest("request exists");
        }

        var alreadyFriends = await _applicationDbContext.Friendships
            .AnyAsync(f =>
                f.User1.Id == userId && f.User2.Id == recipient.Id ||
                f.User1.Id == recipient.Id && f.User2.Id == userId);

        if (alreadyFriends)
        {
            return BadRequest("Already friends");
        }

        var requester = await _applicationDbContext.Users
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (requester is null)
        {
            return BadRequest("null requester");
        }

        var friendRequest = new FriendRequest
        {
            Requester = requester,
            Recipient = recipient,
            CreatedAt = DateTimeOffset.Now
        };

        _applicationDbContext.FriendRequests.Add(friendRequest);
        await _applicationDbContext.SaveChangesAsync();

        return Ok(new FriendRequestResultDto { Successful = true });
    }
}
