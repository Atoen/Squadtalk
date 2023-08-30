using Microsoft.EntityFrameworkCore;
using Squadtalk.Server.Models;

namespace Squadtalk.Server.Services;

public class MessageService
{
    private readonly AppDbContext _dbContext;

    public MessageService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task StoreMessage(Message message)
    {
        await _dbContext.Messages.AddAsync(message);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {

        }
    }
}