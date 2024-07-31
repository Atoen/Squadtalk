using Microsoft.AspNetCore.SignalR;
using Shared.Data.TypedIds;
using Squadtalk.Data;
using Squadtalk.Data.Entities;
using Squadtalk.Hubs;
using Squadtalk.Repositories;

namespace Squadtalk.Services;

public class ChannelCreator
{
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly ChatConnectionManager _connectionManager;
    private readonly SystemMessageService _systemMessageService;
    private readonly ChannelRepository _channelRepository;
    private readonly UserRepository _userRepository;

    public ChannelCreator(
        IHubContext<ChatHub, IChatClient> hubContext,
        ChatConnectionManager connectionManager,
        SystemMessageService systemMessageService,
        ChannelRepository channelRepository,
        UserRepository userRepository)
    {
        _hubContext = hubContext;
        _connectionManager = connectionManager;
        _systemMessageService = systemMessageService;
        _channelRepository = channelRepository;
        _userRepository = userRepository;
    }

    public async Task<Channel?> CreateChannelAsync(
        UserId creatingUserId,
        List<UserId> participantIds,
        CancellationToken cancellationToken = default)
    {
        if (!participantIds.Contains(creatingUserId))
        {
            return null;
        }

        var participants = await _userRepository.GetUserListAsync(participantIds);
        var creatingUser = participants.FirstOrDefault(x => x.Id == creatingUserId);
        if (creatingUser is null)
        {
            return null;
        }

        var channel = await _channelRepository.CreateChannelAsync(participants, cancellationToken);

        if (channel is null)
        {
            return null;
        }

        await NotifyParticipantsAsync(channel, creatingUser);

        return channel;
    }

    private async Task NotifyParticipantsAsync(Channel channel, ApplicationUser creatingUser)
    {
        foreach (var user in channel.Participants)
        {
            var userConnections = _connectionManager.GetUserConnections(user);
            foreach (var connection in userConnections)
            {
                await _hubContext.Groups.AddToGroupAsync(connection, channel.Id);
            }
        }

        await _hubContext.Clients.Groups(channel.Id).AddedToChannel(channel.ToDto());

        if (channel.Participants.Count > 2)
        {
            await _systemMessageService.SendChannelCreatedMessageAsync(creatingUser, channel.Id);
        }
    }
}
