using Microsoft.EntityFrameworkCore;
using Shared.Data.TypedIds;
using Squadtalk.Data;
using Squadtalk.Data.Entities;
using Squadtalk.Data.TypedIds;
using Squadtalk.Services;
using tusdotnet.Interfaces;

namespace Squadtalk.Repositories;

public class FileRepository(
    ApplicationDbContext dbContext,
    TusHelper tusHelper,
    ILogger<FileRepository> logger) : RepositoryBase(dbContext, logger)
{
    public async Task<ITusFile?> GetTusFileAsync(ChannelId channelId, TusFileId fileId, CancellationToken cancellationToken = default)
    {
        var dbFile = await GetFileAsync(channelId, fileId);
        if (dbFile is null)
        {
            return null;
        }

        return await tusHelper.DiskStore.GetFileAsync(dbFile.TusId, cancellationToken);
    }

    public async Task<DbFile?> AddFileAsync(ITusFile tusFile, ChannelId channelId, CancellationToken cancellationToken = default)
    {
        var file = new DbFile
        {
            TusId = new TusFileId(tusFile.Id),
            ChannelId = channelId
        };

        var added = await AddFileAsync(file, cancellationToken);

        return added ? file : null;
    }

    public async Task<bool> AddFileAsync(DbFile file, CancellationToken cancellationToken = default)
    {
        await DbContext.Files.AddAsync(file, cancellationToken);

        return await SaveChangesAsync(cancellationToken);
    }

    public Task<DbFile?> GetFileAsync(ChannelId channelId, TusFileId fileId)
    {
        return FileByChannelPathAsync(DbContext, channelId, fileId);
    }

    private static readonly Func<ApplicationDbContext, ChannelId, TusFileId, Task<DbFile?>> FileByChannelPathAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, ChannelId channelId, TusFileId fileId) => context.Files
                .AsNoTracking()
                .Where(x => x.ChannelId == channelId)
                .SingleOrDefault(x => x.TusId == fileId));
}
