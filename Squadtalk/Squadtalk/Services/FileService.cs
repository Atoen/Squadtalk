using Microsoft.EntityFrameworkCore;
using Shared.Data.TypedIds;
using Squadtalk.Data;
using Squadtalk.Data.Entities;
using Squadtalk.Data.TypedIds;
using tusdotnet.Interfaces;

namespace Squadtalk.Services;

public class FileStorageService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(ApplicationDbContext dbContext, ILogger<FileStorageService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task StoreFileAsync(ITusFile tusFile, ChannelId channelId)
    {
        var file = new DbFile
        {
            ChannelId = new NewChannelId(Guid.Parse(channelId.Value)),
            TusId = new TusFileId(tusFile.Id)
        };

        await _dbContext.Files.AddAsync(file);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            _logger.LogError(e, "Failed to store file");
        }
    }
}