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
    public async Task<ITusFile?> GetTusFileAsync(GroupId groupId, TusFileId fileId, CancellationToken cancellationToken = default)
    {
        var dbFile = await GetFileAsync(groupId, fileId);
        if (dbFile is null)
        {
            return null;
        }

        return await tusHelper.DiskStore.GetFileAsync(dbFile.TusId, cancellationToken);
    }

    public async Task<DbFile?> AddFileAsync(ITusFile tusFile, GroupId groupId, CancellationToken cancellationToken = default)
    {
        var file = new DbFile
        {
            TusId = new TusFileId(tusFile.Id),
            GroupId = groupId
        };

        var added = await AddFileAsync(file, cancellationToken);

        return added ? file : null;
    }

    public async Task<bool> AddFileAsync(DbFile file, CancellationToken cancellationToken = default)
    {
        await DbContext.Files.AddAsync(file, cancellationToken);

        return await SaveChangesAsync(cancellationToken);
    }

    public Task<DbFile?> GetFileAsync(GroupId groupId, TusFileId fileId)
    {
        return FileByGroupPathAsync(DbContext, groupId, fileId);
    }

    private static readonly Func<ApplicationDbContext, GroupId, TusFileId, Task<DbFile?>> FileByGroupPathAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, GroupId groupId, TusFileId fileId) => context.Files
                .AsNoTracking()
                .Where(x => x.GroupId == groupId)
                .SingleOrDefault(x => x.TusId == fileId));
}
