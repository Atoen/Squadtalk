using System.Net;
using Polly;
using Polly.Registry;
using Polly.Retry;
using RestSharp;
using Squadtalk.Data;
using Squadtalk.Extensions;

namespace Squadtalk.Services.Scheduling;

public class UpdateDnsRecords : IDnsRecordUpdater
{
    private readonly IPService _ipService;
    private readonly ResiliencePipelineRegistry<string> _registry;
    private readonly RestClient _restClient;
    private readonly DnsRecordUpdaterStateManager _stateManager;
    private readonly ILogger<UpdateDnsRecords> _logger;
    
    private readonly string _zoneToken;
    private readonly string _zoneId;
    private readonly List<DnsRecord> _records;
    
    public UpdateDnsRecords(IConfiguration configuration, IPService ipService,
        ResiliencePipelineRegistry<string> registry, RestClient restClient,
        DnsRecordUpdaterStateManager stateManager, ILogger<UpdateDnsRecords> logger)
    {
        _ipService = ipService;
        _registry = registry;
        _restClient = restClient;
        _stateManager = stateManager;
        _logger = logger;

        _zoneToken = configuration.GetString("DDns:Token");
        _zoneId = configuration.GetString("DDns:Zone");

        _records = configuration.GetSection("DDns:Records").Get<List<DnsRecord>>()
            ?? throw new ArgumentNullException(nameof(configuration), "DDns:Records");
    }
    
    public async Task Invoke()
    {
        _logger.LogDebug("Retrieving IP address...");
        var pipeline1 = GetPipelineIP();
        var ip = await pipeline1.ExecuteAsync((ipService, _) => ipService.GetIPAddressAsync(), _ipService);

        _stateManager.SetIp(ip);
        
        if (ip is null)
        {
            _logger.LogError("Failed to retrieve IP address");
            return;
        }
        
        _logger.LogDebug("Successfully retrieved IP address: {IP}", ip);
        if (!_stateManager.ShouldUpdateRecords)
        {
            _logger.LogDebug("DNS records are up to date");
            return;
        }

        _logger.LogInformation("Updating DNS records...");
        var pipeline2 = GetPipelineCF();

        try
        {
            await pipeline2.ExecuteAsync((address, _) => UpdateRecords(address), ip);
            _stateManager.Success();
            _logger.LogInformation("Successfully updated {Count} DNS records", _records.Count);
        }
        catch (Exception e)
        {
            _stateManager.Fail();
            _logger.LogError(e, "Failed to update DNS records");
        }
    }

    private async ValueTask UpdateRecords(IPAddress address)
    {
        var addressString = address.ToString();
        var updateTasks = _records.Select(x => UpdateRecord(addressString, x));
        await Task.WhenAll(updateTasks);
    }

    private async Task UpdateRecord(string address, DnsRecord record)
    {
        var request = new RestRequest(
                $"https://api.cloudflare.com/client/v4/zones/{_zoneId}/dns_records/{record.Id}")
            .AddHeader("Authorization", $"bearer {_zoneToken}")
            .AddBody(new
            {
                content = address,
                name = record.Name,
                proxied = true,
                type = "A"
            });

        await _restClient.PatchAsync(request);
        
        _logger.LogDebug("Updated '{Record}' record", record.Name);
    }

    private ResiliencePipeline GetPipelineIP()
    {
        return _registry.GetOrAddPipeline("ddns-ip", builder =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().HandleResult(result => result is not IPAddress),
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(4),
                UseJitter = true,
                MaxRetryAttempts = 10
            });
        });
    }
    
    private ResiliencePipeline GetPipelineCF()
    {
        return _registry.GetOrAddPipeline("ddns-cf", builder =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(10),
                UseJitter = true,
                MaxRetryAttempts = 3
            });
        });
    }
}