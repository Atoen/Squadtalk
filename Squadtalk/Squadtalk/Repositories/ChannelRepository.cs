using Microsoft.EntityFrameworkCore;
using Shared.Data.TypedIds;
using Shared.Models;
using Squadtalk.Data;
using Squadtalk.Data.Entities;
using Squadtalk.Extensions;

namespace Squadtalk.Repositories;

public class ChannelRepository(
    ApplicationDbContext dbContext,
    ILogger<ChannelRepository> logger)
    : RepositoryBase(dbContext, logger)
{
    public Task<Channel?> GetChannelAsync(ChannelId channelId)
    {
        return ChannelByIdAsync(DbContext, channelId);
    }

    public async Task<bool> UserParticipatesInChannelAsync(UserId userId, ChannelId channelId)
    {
        if (channelId == GroupChatModel.GlobalChatId)
        {
            return true;
        }

        var channel = await GetChannelAsync(channelId);

        return channel?.UserParticipatesInChannel(userId) ?? false;
    }

    public async Task<Channel?> CreateChannelAsync(List<ApplicationUser> participants, CancellationToken cancellationToken = default)
    {
        if (participants.Count < 1 || participants.DistinctBy(x => x.Id).Count() != participants.Count)
        {
            return null;
        }

        var channel = new Channel
        {
            Id = ChannelId.New(),
            Participants = participants
        };

        var added = await AddChannelAsync(channel, cancellationToken);

        return added ? channel : null;
    }

    public async Task<bool> AddUsersToGroupAsync(ChannelId channelId, List<ApplicationUser> users)
    {
        if (users.Count == 0)
        {
            return false;
        }

        var channel = await ChannelByIdAsync(DbContext, channelId);
        if (channel is null)
        {
            return false;
        }

        var existingUserIds = channel.Participants.Select(p => p.Id).ToHashSet();
        var newUsers = users
            .Where(user => !existingUserIds.Contains(user.Id))
            .ToList();

        if (newUsers.Count == 0)
        {
            return false;
        }

        channel.Participants.AddRange(newUsers);
        return await UpdateChannelAsync(channel);
    }

    public async Task<bool> AddChannelAsync(Channel channel, CancellationToken cancellationToken = default)
    {
        DbContext.Channels.Add(channel);

        return await SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UpdateChannelAsync(Channel channel, CancellationToken cancellationToken = default)
    {
        DbContext.Channels.Update(channel);

        return await SaveChangesAsync(cancellationToken);
    }

    private static readonly Func<ApplicationDbContext, ChannelId, Task<Channel?>> ChannelByIdAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, ChannelId channelId) => context.Channels
                .Include(x => x.Participants)
                .SingleOrDefault(x => x.Id == channelId));
}
