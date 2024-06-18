using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Shared.Data;
using Shared.DTOs;
using Shared.Services;
using Squadtalk.Data;
using Squadtalk.Hubs;

namespace Squadtalk.Services;

public class LocalChannelCreator : ICreateTextChannelRequestHandler
{
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly ChatConnectionManager _connectionManager;
    private readonly ApplicationDbContext _dbContext;

    public LocalChannelCreator(
        IHubContext<ChatHub, IChatClient> hubContext,
        ChatConnectionManager connectionManager,
        ApplicationDbContext dbContext)
    {
        _hubContext = hubContext;
        _connectionManager = connectionManager;
        _dbContext = dbContext;
    }
    
    public async Task<ChannelId?> CreateTextChannelAsync(List<UserId> participants)
    {
        if (await CreateChannelAsync(participants) is not { } channel) return null;

        await NotifyParticipantsAsync(channel);

        return channel.Id;
    }

    private async Task NotifyParticipantsAsync(Channel channel)
    {
        var dto = new ChannelDto
        {
            Id = channel.Id,
            Participants = channel.Participants.Select(x => x.ToDto()).ToList()
        };
        
        foreach (var user in channel.Participants)
        {
            var userConnections = _connectionManager.GetUserConnections(user);
            foreach (var connection in userConnections)
            {
                await _hubContext.Groups.AddToGroupAsync(connection, channel.Id);
                await _hubContext.Clients.Client(connection).AddedToChannel(dto);
            }
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
            Id = ChannelId.New,
            Participants = participants,
        };

        await _dbContext.Channels.AddAsync(channel);
        await _dbContext.SaveChangesAsync();

        return channel;
    }
}
