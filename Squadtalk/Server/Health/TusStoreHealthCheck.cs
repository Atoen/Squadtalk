using Microsoft.Extensions.Diagnostics.HealthChecks;
using Squadtalk.Server.Services;
using tusdotnet.Stores;

namespace Squadtalk.Server.Health;

public class TusStoreHealthCheck : IHealthCheck
{
    private readonly TusDiskStoreHelper _diskStoreHelper;

    public TusStoreHealthCheck(TusDiskStoreHelper diskStoreHelper)
    {
        _diskStoreHelper = diskStoreHelper;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
    {
        try
        {
            var store = new TusDiskStore(_diskStoreHelper.Path);
            var id = await store.CreateFileAsync(0, "", cancellationToken);
            await store.DeleteFileAsync(id, cancellationToken);

            return HealthCheckResult.Healthy();
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy(exception: e);
        }
    }
}