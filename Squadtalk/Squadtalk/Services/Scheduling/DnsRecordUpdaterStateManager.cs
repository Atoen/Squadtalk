using System.Net;

namespace Squadtalk.Services.Scheduling;

public class DnsRecordUpdaterStateManager
{
    public bool ShouldUpdateRecords =>
        !UpdateWasSuccessful ||
        (IpAddress is not null && !IpAddress.Equals(_lastAddress)) ||
        DateTime.Now.Subtract(_updateTimestamp) > ForcedDnsUpdateTimeSpan;

    public bool UpdateWasSuccessful { get; private set; }
    
    public TimeSpan ForcedDnsUpdateTimeSpan { get; set; } = TimeSpan.FromMinutes(1);
    
    public IPAddress? IpAddress { get; private set; }

    private IPAddress? _lastAddress;
    private DateTime _updateTimestamp;

    public void Success()
    {
        UpdateWasSuccessful = true;
        _updateTimestamp = DateTime.Now;
    }

    public void Fail() => UpdateWasSuccessful = false;

    public void SetIp(IPAddress? address)
    {
        _lastAddress = IpAddress;
        IpAddress = address;
    }
}