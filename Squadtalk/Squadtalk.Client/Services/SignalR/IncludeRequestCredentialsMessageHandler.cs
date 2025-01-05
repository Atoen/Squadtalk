using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Squadtalk.Client.Services.SignalR;

public class IncludeRequestCredentialsMessageHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.SameOrigin);
        return base.SendAsync(request, cancellationToken);
    }
}