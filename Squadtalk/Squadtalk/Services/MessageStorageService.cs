using Microsoft.EntityFrameworkCore;
using Shared.Communication;
using Shared.Data.TypedIds;
using Squadtalk.Data;
using Squadtalk.Data.Entities;
using Squadtalk.Extensions;

namespace Squadtalk.Services;

public class MessageStorageService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly LocalMessageNotificationService _notificationService;
    private readonly ILogger<MessageStorageService> _logger;

    public MessageStorageService(
        ApplicationDbContext dbContext,
        LocalMessageNotificationService notificationService,
        ILogger<MessageStorageService> logger)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _logger = logger;
    }

    public Message CreateMessage(ApplicationUser author, string content, ChannelId id) => new()
    {
        Author = author,
        Content = content,
        ChannelId = id,
        Timestamp = DateTimeOffset.Now
    };
    
    public async Task StoreMessageAsync(Message message)
    {
        await _dbContext.Messages.AddAsync(message);

        if (message.ChannelId != GroupChatModel.GlobalChatId)
        {
            var channel = await _dbContext.Channels
                .AsTracking()
                .FirstOrDefaultAsync(x => x.Id == message.ChannelId);
            
            channel?.WithLastMessage(message);
        }
        
        try
        {
            await _dbContext.SaveChangesAsync();
            // await _notificationService.NotifyAboutMessageAsync(message);
        }
        catch (DbUpdateException e)
        {
            _logger.LogError(e, "Failed to store message");
        }
    }
}