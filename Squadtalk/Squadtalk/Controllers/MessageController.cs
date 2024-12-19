using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.TypedIds;
using Shared.Extensions;
using Shared.Routing;
using Squadtalk.Data.Entities;
using Squadtalk.Repositories;
using Squadtalk.Services;

namespace Squadtalk.Controllers;

[Authorize]
[ApiController]
[Route(Routes.Endpoints.MessageController)]
public class MessageController : ControllerBase
{
    private readonly MessageRepository _messageRepository;
    private readonly ChannelRepository _channelRepository;
    private readonly ChannelCreator _channelCreator;

    public MessageController(
        ChannelCreator channelCreator,
        MessageRepository messageRepository,
        ChannelRepository channelRepository)
    {
        _channelCreator = channelCreator;
        _messageRepository = messageRepository;
        _channelRepository = channelRepository;
    }

    [HttpGet(Routes.RelativeEndpoints.GetMessages)]
    public async Task<ActionResult<List<Message>>> GetMessages(ChannelId channelId, string? timestamp)
    {
        var userId = HttpContext.User.GetUserId();
        var userParticipates = await _channelRepository.UserParticipatesInChannelAsync(userId, channelId);
        if (!userParticipates)
        {
            return Forbid();
        }

        var messages = await _messageRepository.GetPageAsync(channelId, timestamp, HttpContext.RequestAborted);
        return messages;
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
}
