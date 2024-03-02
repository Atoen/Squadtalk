using Microsoft.EntityFrameworkCore;
using Shared.Communication;
using Squadtalk.Data;
using Squadtalk.Extensions;

namespace Squadtalk.Services;

public class MessageStorageService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<MessageStorageService> _logger;

    public MessageStorageService(ApplicationDbContext dbContext, ILogger<MessageStorageService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public Message CreateMessage(ApplicationUser author, string content, string channelId) => new()
    {
        Author = author,
        Content = content,
        ChannelId = channelId,
        Timestamp = DateTimeOffset.Now
    };
    
    public async Task StoreMessageAsync(Message message)
    {
        await _dbContext.Messages.AddAsync(message);

        if (message.ChannelId != GroupChat.GlobalChatId)
        {
            var channel = await _dbContext.Channels
                .AsTracking()
                .FirstOrDefaultAsync(x => x.Id == message.ChannelId);
            
            channel?.WithLastMessage(message);
        }
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            _logger.LogError(e, "1 Failed to store message with id: {Id}", message.Id);
        }
    }
}