using Squadtalk.Data;

namespace Squadtalk.Repositories;

public abstract class RepositoryBase
{
    protected readonly ApplicationDbContext DbContext;
    protected readonly ILogger Logger;

    protected RepositoryBase(ApplicationDbContext dbContext, ILogger logger)
    {
        DbContext = dbContext;
        Logger = logger;
    }

    protected async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await DbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to save changes");
        }

        return false;
    }
}
