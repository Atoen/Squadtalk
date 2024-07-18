using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Shared.Data.TypedIds;
using Squadtalk.Data;
using Squadtalk.Data.Entities;
using Squadtalk.Hubs;

namespace Squadtalk.Services;

public class ChannelCreator
{
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly ChatConnectionManager _connectionManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly SystemMessageService _systemMessageService;

    public ChannelCreator(
        IHubContext<ChatHub, IChatClient> hubContext,
        ChatConnectionManager connectionManager,
        ApplicationDbContext dbContext,
        SystemMessageService systemMessageService)
    {
        _hubContext = hubContext;
        _connectionManager = connectionManager;
        _dbContext = dbContext;
        _systemMessageService = systemMessageService;
    }

    public async Task<ChannelId?> CreateChannelAsync(ApplicationUser creatingUser, IEnumerable<UserId> participants)
    {
        var channel = await CreateChannelAsync(participants.ToList());
        if (channel is null)
        {
            return null;
        }

        await NotifyParticipantsAsync(channel, creatingUser);

        return channel.Id;
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

    private async Task<Channel?> CreateChannelAsync(List<UserId> participantsId)
    {
        if (participantsId.Count < 2 || participantsId.Distinct().Count() != participantsId.Count)
        {
            return null;
        }

        var participants = await _dbContext.Users
            .Where(x => participantsId.Contains(x.Id))
            .ToListAsync();

        if (participants.Count != participantsId.Count)
        {
            return null;
        }

        var channel = new Channel
        {
            Id = ChannelId.New(),
            Participants = participants,
        };

        await _dbContext.Channels.AddAsync(channel);
        await _dbContext.SaveChangesAsync();

        return channel;
    }
}
