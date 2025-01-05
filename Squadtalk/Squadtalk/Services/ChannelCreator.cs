using Microsoft.AspNetCore.SignalR;
using Shared.Data.TypedIds;
using Squadtalk.Data;
using Squadtalk.Data.Entities;
using Squadtalk.Repositories;
using Squadtalk.Signalr;

namespace Squadtalk.Services;

public class ChannelCreator
{
    private readonly IHubContext<ChatHub, IChatClientOld> _hubContext;
    private readonly HubConnectionManager _connectionManager;
    private readonly SystemMessageService _systemMessageService;
    private readonly ChannelRepository _channelRepository;
    private readonly UserRepository _userRepository;

    public ChannelCreator(
        IHubContext<ChatHub, IChatClientOld> hubContext,
        HubConnectionManager connectionManager,
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
            var userConnections = await _connectionManager.GetUserConnectionsAsync(user);
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
