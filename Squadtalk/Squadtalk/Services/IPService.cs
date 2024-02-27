using System.Net;
using System.Text.RegularExpressions;
using RestSharp;
using Squadtalk.Extensions;

namespace Squadtalk.Services;

public partial class IPService
{
    private readonly RestClient _restClient;
    private readonly string _url;

    public IPService(RestClient restClient, IConfiguration configuration)
    {
        _restClient = restClient;
        _url = configuration.GetString("DDns:Trace");
    }
    
    public async ValueTask<IPAddress?> GetIPAddressAsync()
    {
        var request = new RestRequest(_url);
        var response = await _restClient.ExecuteAsync(request);

        if (!response.IsSuccessful || response.Content is not { } body)
        {
            return null;
        }
        
        var match = MyRegex().Match(body);
        if (!match.Success)
        {
            return null;
        }

        if (!IPAddress.TryParse(match.Groups[1].Value, out var ip))
        {
            return null;
        }

        return ip;
    }

    [GeneratedRegex(@"ip=([0-9\.]+)")]
    private static partial Regex MyRegex();
}