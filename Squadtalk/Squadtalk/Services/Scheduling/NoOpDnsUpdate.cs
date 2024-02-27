namespace Squadtalk.Services.Scheduling;

public class NoOpDnsUpdate : IDnsRecordUpdater
{
    public Task Invoke()
    {
        return Task.CompletedTask;
    }
}