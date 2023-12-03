using Microsoft.EntityFrameworkCore;
using Squadtalk.Data;

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

    public async Task StoreMessageAsync(Message message)
    {
        await _dbContext.Messages.AddAsync(message);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            _logger.LogError(e, "Failed to store message with id: {Id}", message.Id);
        }
    }
}